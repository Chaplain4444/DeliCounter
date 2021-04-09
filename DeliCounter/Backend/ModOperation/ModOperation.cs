﻿using Sentry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace DeliCounter.Backend.ModOperation
{
    public abstract class ModOperation
    {
        internal Mod Mod { get; }
        internal Version VersionNumber { get; }

        internal List<string> InstalledFiles { get; } = new();

        internal Action<double, string> ProgressDialogueCallback { get; set; }

        protected internal bool Completed { get; protected set; }

        protected internal string Message { get; protected set; }

        internal ModOperation(Mod mod, Version versionNumber)
        {
            Mod = mod;
            VersionNumber = versionNumber;
        }

        internal virtual async Task Run()
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["mod"] = new
                {
                    Guid = Mod.Guid,
                    Version = VersionNumber.ToString()
                };
            });
        }
    }
}