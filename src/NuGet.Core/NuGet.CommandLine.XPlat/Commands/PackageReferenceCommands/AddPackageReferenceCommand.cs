﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Commands;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuGet.CommandLine.XPlat
{
    public static class AddPackageReferenceCommand
    {
        public static void Register(CommandLineApplication app, Func<ILogger> getLogger,
            Func<IPackageReferenceCommandRunner> getCommandRunner)
        {
            app.Command("add", addpkg =>
            {
                addpkg.Description = Strings.AddPkg_Description;
                addpkg.HelpOption(XPlatUtility.HelpOption);

                addpkg.Option(
                    CommandConstants.ForceEnglishOutputOption,
                    Strings.ForceEnglishOutput_Description,
                    CommandOptionType.NoValue);

                var id = addpkg.Option(
                    "--package",
                    Strings.AddPkg_PackageIdDescription,
                    CommandOptionType.SingleValue);

                var version = addpkg.Option(
                    "--version",
                    Strings.AddPkg_PackageVersionDescription,
                    CommandOptionType.SingleValue);

                var dgFilePath = addpkg.Option(
                    "-d|--dg-file",
                    Strings.AddPkg_DgFileDescription,
                    CommandOptionType.SingleValue);

                var projectPath = addpkg.Option(
                    "-p|--project",
                    Strings.AddPkg_ProjectPathDescription,
                    CommandOptionType.SingleValue);

                var frameworks = addpkg.Option(
                    "-f|--framework",
                    Strings.AddPkg_FrameworksDescription,
                    CommandOptionType.SingleValue);

                var noRestore = addpkg.Option(
                    "-n|--no-restore",
                    Strings.AddPkg_NoRestoreDescription,
                    CommandOptionType.NoValue);

                var sources = addpkg.Option(
                    "-s|--source",
                    Strings.AddPkg_SourcesDescription,
                    CommandOptionType.SingleValue);

                var packageDirectory = addpkg.Option(
                    "--package-directory",
                    Strings.AddPkg_PackageDirectoryDescription,
                    CommandOptionType.SingleValue);

                addpkg.OnExecute(() =>
                {
                    ValidateArgument(id, addpkg.Name);
                    ValidateArgument(projectPath, addpkg.Name);
                    ValidateProjectPath(projectPath, addpkg.Name);
                    if (!noRestore.HasValue())
                    {
                        ValidateArgument(dgFilePath, addpkg.Name);
                    }
                    var logger = getLogger();
                    var noVersion = !version.HasValue();
                    var packageVersion = version.HasValue() ? version.Value() : "*";
                    var packageDependency = new PackageDependency(id.Values[0], VersionRange.Parse(packageVersion));
                    var packageRefArgs = new PackageReferenceArgs(projectPath.Value(), packageDependency, logger)
                    {
                        Frameworks = MSBuildStringUtility.Split(frameworks.Value()),
                        Sources = MSBuildStringUtility.Split(sources.Value()),
                        PackageDirectory = packageDirectory.Value(),
                        NoRestore = noRestore.HasValue(),
                        NoVersion = noVersion,
                        DgFilePath = dgFilePath.Value()
                    };
                    var msBuild = new MSBuildAPIUtility(logger);
                    var addPackageRefCommandRunner = getCommandRunner();
                    return addPackageRefCommandRunner.ExecuteCommand(packageRefArgs, msBuild);
                });
            });
        }

        private static void ValidateArgument(CommandOption arg, string commandName)
        {
            if (arg.Values.Count < 1)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Error_PkgMissingArgument,
                    commandName,
                    arg.Template));
            }
        }

        private static void ValidateProjectPath(CommandOption projectPath, string commandName)
        {
            if (!File.Exists(projectPath.Value()) || !projectPath.Value().EndsWith("proj", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    Strings.Error_PkgMissingOrInvalidProjectFile,
                    commandName,
                    projectPath.Value()));
            }
        }
    }
}