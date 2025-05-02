namespace AuthorizationServerSample.Eksternal
{
    using Microsoft.AspNetCore.DataProtection;
    using System.Security.Cryptography;
    using System.Xml;

    public class XXEVurnerability
    {
        // This method ensure that the XML processing is secure by disabling DTD processing and handling the XML string safely.
        public RSA LoadRsaFromXml(string publicKeyXml)
        {
            var rsa = RSA.Create();

            // Disable DTD processing for XML security
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit
            };

            using (var reader = XmlReader.Create(new StringReader(publicKeyXml), settings))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);

                // Assuming the XML is valid and properly formatted
                rsa.FromXmlString(xmlDoc.OuterXml);
            }

            return rsa;
        }
    }

}
