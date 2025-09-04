// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Altemiq.IO.Las;
using Microsoft.Extensions.DependencyInjection;

var rootCommand = new RootCommand()
    .AddAllTools()
    .UseServices((parseResult, services) => services.AddSingleton(parseResult.CreateConsole(Options.Output)))
    .AddFiglet(Altemiq.IO.Las.Tool.Properties.Resources.Title, Spectre.Console.Color.DeepSkyBlue1);

return await rootCommand
    .Parse(args)
    .InvokeAsync()
    .ConfigureAwait(false);