﻿using System.Reflection;
using System.Runtime.InteropServices;
using NServiceBus.Persistence.Sql;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Web")]
[assembly: AssemblyProduct("Web")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1d51abeb-2e10-44dd-94d1-a0a23146437e")]
[assembly: SqlPersistenceSettings(MsSqlServerScripts = true)]
