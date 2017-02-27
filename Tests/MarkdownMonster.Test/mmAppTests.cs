using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class mmAppTests
    {
        [TestMethod]
        public void EncryptDecryptStringWithMachineKey()
        {
            string password = "super@Seekrit";

            var encrypted = mmApp.EncryptString(password);
            Assert.IsNotNull(encrypted);
            Console.WriteLine(encrypted);

            var decrypted = mmApp.DecryptString(encrypted);
            Console.WriteLine(decrypted);

            Assert.IsTrue(password == decrypted, "initial and two-way values don't match");
        }
    }
}
