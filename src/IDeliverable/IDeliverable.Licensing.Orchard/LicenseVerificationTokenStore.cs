using System;
using IDeliverable.Licensing.VerificationTokens;
using Orchard.FileSystems.AppData;
using Orchard.Logging;

namespace IDeliverable.Licensing.Orchard
{
    internal class LicenseVerificationTokenStore : ILicenseVerificationTokenStore
    {
        public LicenseVerificationTokenStore(IAppDataFolder appDataFolder)
        {
            mAppDataFolder = appDataFolder;
        }

        private readonly IAppDataFolder mAppDataFolder;

        public LicenseVerificationToken Load(string productId)
        {
            var path = GetRelativeTokenFilePath(productId);
            if (!mAppDataFolder.FileExists(path))
                return null;

            LicenseVerificationToken token = null;
            var tokenBase64 = mAppDataFolder.ReadFile(path);

            try
            {
                token = LicenseVerificationToken.FromBase64(tokenBase64);
            }
            catch
            {
                // An exception while parsing the token indicates somebody probably
                // tampered with it or it got corrupted. In this case, let's clear out
                // the store and return nothing.
                Clear(productId);
            }

            return token;
        }

        public void Save(string productId, LicenseVerificationToken token)
        {
            Clear(productId);

            var path = GetRelativeTokenFilePath(productId);

            var tokenBase64 = token.ToBase64();
            mAppDataFolder.CreateFile(path, tokenBase64);
        }

        public void Clear(string productId)
        {
            var path = GetRelativeTokenFilePath(productId);
            if (mAppDataFolder.FileExists(path))
                mAppDataFolder.DeleteFile(path);
        }

        private string GetRelativeTokenFilePath(string productId)
        {
            return $"IDeliverable.Licensing/{productId}.lic";
        }
    }
}