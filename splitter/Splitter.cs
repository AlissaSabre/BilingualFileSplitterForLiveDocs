using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using disfr.Doc;

namespace splitter
{
    /// <summary>Service class to split a bilingual file into two monolingual XML files.</summary>
    public class Splitter
    {
        public bool Verbose { get; set; }

        /// <summary>Splits a bilingual file into a set of monolingual XML files.</summary>
        /// <param name="filename"></param>
        public void Split(string filename)
        {
            var bundle = ReaderManager.Current.Read(filename);
            var assets = bundle.Assets as IList<IAsset> ?? bundle.Assets.ToArray();
            if (assets.Count == 0)
            {
                throw new IOException($"Contains no usable contents: {filename}");
            }
            foreach (var asset in assets)
            {
                var basename = $"{Path.GetFileName(filename)}-{Path.GetFileName(asset.Original)}";
                Emit(basename, asset, asset.SourceLang, pair => pair.Source);
                Emit(basename, asset, asset.TargetLang, pair => pair.Target);
            }
        }

        // Element and attribute names used in the output XML.

        private static readonly XName XmlLang = XNamespace.Xml + "lang";

        private static readonly XName XmlSpace = XNamespace.Xml + "space";

        private static readonly XName Id = "id";

        private static readonly XName File = "file";

        private static readonly XName Name = "name";

        private static readonly XName Seg = "seg";

        private static readonly XName Tag = "tag";

        /// <summary>Produces and outputs an XML file.</summary>
        /// <param name="basename">Base name of an output file.</param>
        /// <param name="asset">Asset for output.</param>
        /// <param name="language">The language the contents of the XML file is in.</param>
        /// <param name="inline_selector">Delegate to extract an <see cref="InlineString"/> from an <see cref="ITransPair"/>.</param>
        private void Emit(string basename, IAsset asset, string language,
            Func<ITransPair, InlineString> inline_selector)
        {
            new XElement(File,
                new XAttribute(XmlLang, language),
                new XAttribute(Name, basename),
                "\n",
                asset.TransPairs.Where(IsSegment).Select(pair =>
                    new object[]
                    {
                        new XElement(Seg,
                            new XAttribute(Id, pair.Id),
                            new XAttribute(XmlSpace, "preserve"),
                            Convert(inline_selector(pair))),
                        "\n"
                    }))
                .Save($"{basename} ({language}).xml", SaveOptions.DisableFormatting);
        }

        /// <summary>Tests whether an <see cref="ITransPair"/> object is for a segment.</summary>
        /// <param name="pair">A translation pair.</param>
        /// <returns>True if it is for a segment. False if it is for an inter-segment content.</returns>
        private bool IsSegment(ITransPair pair) => (pair.Serial > 0);

        /// <summary>Converts an <see cref="InlineString"/> to a mixed content of an XML element.</summary>
        /// <param name="inline"><see cref="InlineString"/> to be converted.</param>
        /// <returns>Series of <see cref="XNode"/> to represent a mixed content.</returns>
        private IEnumerable<XNode> Convert(InlineString inline)
        {
            foreach (var rwp in inline.RunsWithProperties)
            {
                // simulate Render.HideDel by ourselves
                if ((rwp.Property & InlineProperty.Del) == 0)
                {
                    switch (rwp.Run)
                    {
                        case InlineText text:
                            yield return new XText(text.ToString());
                            break;
                        case InlineTag _:
                            yield return new XElement(Tag);
                            break;
#if DEBUG
                        default:
                            Console.Error.WriteLine("unknown inline run: "
                                + rwp.Run.ToString(InlineString.RenderDebug));
                            break;
#endif
                    }
                }
            }
        }
    }
}
