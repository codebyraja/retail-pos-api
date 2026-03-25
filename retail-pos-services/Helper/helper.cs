using System.Xml.Serialization;
using System.Xml;
using System.Text;
using System.Text.Json;

namespace QSRHelperApiServices.Helper
{
    public class Helper
    {
        public static string GetBaseUrl(HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host}";
        }

        public dynamic GenerateSku()
        {
            // Example: SKU-20250526-AB12
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var randomPart = new string(
                Enumerable.Range(0, 4).Select(_ => chars[random.Next(chars.Length)]).ToArray()
            );
            var sku = $"SKU-{datePart}-{randomPart}";
            return sku;
        }

        public string GenerateUniqueBarcode1111(int productId)
        {
            return $"{DateTime.UtcNow:yyyyMMddHHmmss}-{productId}";
        }

        public string GenerateUniqueBarcode(string? entityType, int entityId)
        {
            // Prefix: first 3 letters of entity type (upper case)
            string prefix = entityType?.Length >= 3 ? entityType.Substring(0, 3).ToUpper() : entityType.ToUpper();

            // Example Format: PRD-20251025-000123
            string barcode = $"{prefix}-{DateTime.Now:yyyyMMddHHmmss}-{entityId:D6}";

            return barcode;
        }


        public string GenerateUniqueFileName(string name, string originalFileName)
        {
            var sanitized = Path.GetFileNameWithoutExtension(name).Replace(" ", "_");
            var extension = Path.GetExtension(originalFileName);
            return $"{sanitized}_{Guid.NewGuid():N}{extension}";
        }

        public string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '-');
            }
            return name.Replace(" ", "-");
        }

        public static double ParseToDouble(JsonElement element)
        {
            try
            {
                if (element.ValueKind == JsonValueKind.Number)
                    return element.GetDouble();

                if (element.ValueKind == JsonValueKind.String && double.TryParse(element.GetString(), out double result))
                    return result;
            }
            catch
            {
                // Log if necessary
            }

            return 0; // default fallback
        }


        //public static string CreateXML(object YourClassObject)
        //{
        //    XmlDocument xmlDoc = new XmlDocument(); //Represents an XML document,
        //                                            // Initializes a new instance of the XmlDocument class.
        //    XmlSerializer xmlSerializer = new XmlSerializer(YourClassObject.GetType());
        //    // Creates a stream whose backing store is memory.
        //    using (MemoryStream xmlStream = new MemoryStream())
        //    {
        //        xmlSerializer.Serialize(xmlStream, YourClassObject);
        //        xmlStream.Position = 0;
        //        //Loads the XML document from the specified string.
        //        xmlDoc.Load(xmlStream);
        //        return xmlDoc.InnerXml;
        //    }
        //}

        //        public static string CreateXml(object yourClassObject)
        //        {
        //            try
        //            {
        //                XmlDocument xmlDoc = new XmlDocument();
        //                XmlSerializer xmlSerializer = new XmlSerializer(yourClassObject.GetType());

        //                using (MemoryStream xmlStream = new MemoryStream())
        //                {
        //                    xmlSerializer.Serialize(xmlStream, yourClassObject);
        //                    xmlStream.Position = 0;

        //                    xmlDoc.Load(xmlStream);

        //                    // Optionally, include XML declaration
        //                    //XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
        //                    //xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);

        //                    return xmlDoc.InnerXml;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                // Handle serialization exceptions
        //                Console.WriteLine($"Error in CreateXml: {ex.Message}");
        //#pragma warning disable CS8603 // Possible null reference return.
        //                return null;
        //#pragma warning restore CS8603 // Possible null reference return.
        //            }
        //        }

        public static string CreateXml<T>(T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true, // ✅ Avoid encoding declaration
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, obj);
                    return textWriter.ToString();
                }
            }
        }

    }
}
