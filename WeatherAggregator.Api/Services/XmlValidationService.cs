using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

namespace WeatherAggregator.Api.Services
{
    public class XmlValidationService
    {
        private readonly XmlSchemaSet _schemas = new XmlSchemaSet();
        private readonly string _xsdLoadErrorMessage = string.Empty;

        public XmlValidationService()
        {
            // XSD Yükleme (Bu kısım doğru, dokunmuyoruz)
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(name => name.EndsWith("WeatherReport.xsd"));
                if (!string.IsNullOrEmpty(resourceName))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
                    using (var reader = XmlReader.Create(stream)) { _schemas.Add(null, reader); }
                }
                else { _xsdLoadErrorMessage = "Embedded XSD not found."; }
            }
            catch (Exception ex) { _xsdLoadErrorMessage = "Error loading XSD: " + ex.ToString(); }
        }

        public (bool, List<string>) ValidateWithXsd(string xmlContent)
        {
            // Bu metod doğru, dokunmuyoruz.
            var errors = new List<string>();
            if (!string.IsNullOrEmpty(_xsdLoadErrorMessage)) { errors.Add(_xsdLoadErrorMessage); return (false, errors); }
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);
                xmlDoc.Schemas = _schemas;
                xmlDoc.Validate((sender, args) => { if (args.Severity == XmlSeverityType.Error) errors.Add(args.Message); });
                return (errors.Count == 0, errors);
            }
            catch (Exception ex) { errors.Add($"XML parsing error: {ex.Message}"); return (false, errors); }
        }

        public (bool, List<string>) ValidateWithDtd(string xmlContent)
        {
            var errors = new List<string>();
            try
            {
                // === DÜZELTME BURADA ===
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Parse,
                    ValidationType = ValidationType.DTD
                };

                // Olay dinleyicisini '+=' ile ekliyoruz.
                settings.ValidationEventHandler += (sender, args) => {
                    if (args.Severity == XmlSeverityType.Error) errors.Add(args.Message);
                };
                // ========================

                string dtdRules = @"
                    <!ELEMENT WeatherReport (City, Temperature, Unit, Condition, Humidity, WindSpeed, Source)>
                    <!ELEMENT City (#PCDATA)> <!ELEMENT Temperature (#PCDATA)> <!ELEMENT Unit (#PCDATA)>
                    <!ELEMENT Condition (#PCDATA)> <!ELEMENT Humidity (#PCDATA)> <!ELEMENT WindSpeed (#PCDATA)>
                    <!ELEMENT Source (#PCDATA)>
                ";

                string cleanedXml = Regex.Replace(xmlContent, @"xmlns(:\w+)?\s*=\s*""[^""]*""", "", RegexOptions.IgnoreCase);
                if (cleanedXml.TrimStart().StartsWith("<?xml"))
                {
                    cleanedXml = cleanedXml.Substring(cleanedXml.IndexOf('>') + 1).Trim();
                }

                var fullXml = $"<!DOCTYPE WeatherReport [{dtdRules}]>{Environment.NewLine}{cleanedXml}";

                using (var stringReader = new StringReader(fullXml))
                using (var xmlReader = XmlReader.Create(stringReader, settings))
                {
                    while (xmlReader.Read()) { }
                }

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"XML parsing or DTD validation error: {ex.Message}");
                return (false, errors);
            }
        }
    }
}