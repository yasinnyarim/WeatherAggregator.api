using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

namespace WeatherAggregator.Api.Services
{
    public class XsltTransformService
    {
        private readonly XslCompiledTransform _xslTransform;
        private readonly bool _isLoaded = false;
        private readonly string _loadErrorMessage = "Stylesheet could not be loaded.";

        public XsltTransformService()
        {
            _xslTransform = new XslCompiledTransform();
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(str => str.EndsWith("WeatherReport.xslt"));
                if (string.IsNullOrEmpty(resourceName))
                {
                    _loadErrorMessage = "Embedded resource 'WeatherReport.xslt' not found. Ensure its Build Action is 'Embedded Resource'.";
                    return;
                }
                using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = XmlReader.Create(stream);
                _xslTransform.Load(reader);
                _isLoaded = true;
            }
            catch (Exception ex) { _loadErrorMessage = ex.ToString(); }
        }

        public string TransformXml(string xmlContent)
        {
            if (!_isLoaded) return $"<html><body><h1>Error</h1><pre>{_loadErrorMessage}</pre></body></html>";
            try
            {
                using var sr = new StringReader(xmlContent);
                using var xr = XmlReader.Create(sr);
                using var sw = new StringWriter();
                _xslTransform.Transform(xr, null, sw);
                return sw.ToString();
            }
            catch (Exception ex) { return $"<html><body><h1>Transform Error</h1><pre>{ex.ToString()}</pre></body></html>"; }
        }
    }
}