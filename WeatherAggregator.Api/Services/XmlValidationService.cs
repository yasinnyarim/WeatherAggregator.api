using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace WeatherAggregator.Api.Services
{
    public class XmlValidationService
    {
        // Uyarıyı çözmek için alanı doğrudan burada başlatıyoruz.
        private readonly XmlSchemaSet _schemas = new XmlSchemaSet();
        private readonly string _loadErrorMessage = string.Empty;

        public XmlValidationService()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(name => name.EndsWith("WeatherReport.xsd"));

                if (string.IsNullOrEmpty(resourceName))
                {
                    _loadErrorMessage = "Embedded resource 'WeatherReport.xsd' not found.";
                    return;
                }

                using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
                using (var reader = XmlReader.Create(stream))
                {
                    _schemas.Add(null, reader);
                }
            }
            catch (Exception ex)
            {
                _loadErrorMessage = "Error loading XSD schema: " + ex.ToString();
            }
        }

        // Metodun imzasını değiştiriyoruz, artık 'out' parametresi yok.
        // Bunun yerine bir Tuple (demet) döndürüyor: (bool IsValid, List<string> Errors)
        public (bool, List<string>) Validate(string xmlContent)
        {
            var errors = new List<string>();
            if (!string.IsNullOrEmpty(_loadErrorMessage))
            {
                errors.Add(_loadErrorMessage);
                return (false, errors);
            }

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);
                xmlDoc.Schemas = _schemas;

                // CS1628 hatasını çözmek için, lambda ifadesi dışarıdaki 'errors' listesini kullanıyor.
                xmlDoc.Validate((sender, args) =>
                {
                    if (args.Severity == XmlSeverityType.Error)
                    {
                        errors.Add(args.Message);
                    }
                });

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"XML parsing error: {ex.Message}");
                return (false, errors);
            }
        }
    }
}