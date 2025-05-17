using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace WsSecurityCore.Security;

/// <summary>
/// Helper class for XML digital signatures used in WS-Security
/// </summary>
public class XmlSignatureHelper
{
    /// <summary>
    /// Signs an XML document using a certificate
    /// </summary>
    /// <param name="xmlDoc">The XML document to sign</param>
    /// <param name="certificate">The certificate to use for signing</param>
    /// <param name="referenceUri">The reference URI for the signature (e.g., "#_body")</param>
    /// <param name="signatureId">The ID to assign to the signature element</param>
    /// <returns>The signed XML document</returns>
    public static XmlDocument SignXmlDocument(XmlDocument xmlDoc, X509Certificate2 certificate, string referenceUri,
        string signatureId)
    {
        SignedXml signedXml = new(xmlDoc);
        var privateKey = certificate.GetRSAPrivateKey();
        if (privateKey == null)
        {
            throw new ArgumentException("Certificate does not contain an RSA private key", nameof(certificate));
        }

        signedXml.SigningKey = privateKey;

        Reference reference = new() { Uri = referenceUri };


        XmlDsigEnvelopedSignatureTransform env = new();
        reference.AddTransform(env);
        signedXml.AddReference(reference);
        KeyInfo keyInfo = new();
        keyInfo.AddClause(new KeyInfoX509Data(certificate));
        signedXml.KeyInfo = keyInfo;
        if (!string.IsNullOrEmpty(signatureId))
        {
            signedXml.Signature.Id = signatureId;
        }

        signedXml.ComputeSignature();

        XmlElement signatureElement = signedXml.GetXml();
        xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(signatureElement, true));

        return xmlDoc;
    }

    /// <summary>
    /// Verifies the signature in an XML document
    /// </summary>
    /// <param name="xmlDoc">The signed XML document</param>
    /// <param name="certificate">The certificate to use for verification</param>
    /// <returns>True if the signature is valid, false otherwise</returns>
    public static bool VerifyXmlSignature(XmlDocument xmlDoc, X509Certificate2 certificate)
    {
        XmlNodeList signatureNodes = xmlDoc.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);
        if (signatureNodes.Count == 0)
        {
            return false;
        }

        SignedXml signedXml = new(xmlDoc);
        var signatureElement = signatureNodes[0] as XmlElement;
        if (signatureElement == null)
        {
            return false;
        }

        signedXml.LoadXml(signatureElement);
        var publicKey = certificate.GetRSAPublicKey();
        if (publicKey == null)
        {
            throw new ArgumentException("Certificate does not contain an RSA public key", nameof(certificate));
        }

        return signedXml.CheckSignature(publicKey);
    }
}