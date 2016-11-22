using ELB.Data.Models;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ELB.Data.Helpers {
	class TypeHelper {
		public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
			while (toCheck != null && toCheck != typeof(object)) {
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur && cur != toCheck) {
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}

		public static Dictionary<Type, Type> CreateTypesForSubclassesOf(Type t) {
			var codeNamespace = new CodeNamespace(t.FullName + ".Generated");
			codeNamespace.Imports.Add(new CodeNamespaceImport("System"));

			var subclasses = t.Assembly.GetTypes().Where(type => type.IsSubclassOf(t));
			foreach (var c in subclasses) {
				var newType = new CodeTypeDeclaration(c.Name) {
					TypeAttributes = TypeAttributes.Public
				};
				newType.BaseTypes.Add("ELB.Data.Models.Generated.Model");

				PropertyInfo[] properties = c.GetProperties();
				foreach (PropertyInfo pi in properties) {
					// ignore the property if it exists in the model base
					if (typeof(ModelBase).GetProperties().Count(x => x.Name == pi.Name) != 0) {
						continue;
					}
					var snippet = new CodeSnippetTypeMember {
						Text = string.Format("public {0} {1} {{ get; set; }}",
							pi.PropertyType.IsPrimitive ? pi.PropertyType.Name : "string",
							pi.Name
						)
					};
					newType.Members.Add(snippet);
				}

				codeNamespace.Types.Add(newType);
			}

			var codeCompileUnit = new CodeCompileUnit();
			codeCompileUnit.Namespaces.Add(codeNamespace);

			var compilerParameters = new CompilerParameters {
				GenerateInMemory = true,
				IncludeDebugInformation = false,
				TreatWarningsAsErrors = true,
				WarningLevel = 4
			};
			compilerParameters.ReferencedAssemblies.Add("System.dll");
			compilerParameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().CodeBase);

			var provider = new CSharpCodeProvider();
			var compilerResults = provider.CompileAssemblyFromDom(compilerParameters, codeCompileUnit);

			if (compilerResults == null) {
				throw new InvalidOperationException("ClassCompiler did not return results.");
			}
			if (compilerResults.Errors.HasErrors) {
				var errors = string.Empty;
				foreach (CompilerError compilerError in compilerResults.Errors) {
					errors += compilerError.ErrorText + "\n";
				}
				throw new InvalidOperationException("Errors while compiling the dynamic classes:\n" + errors);
			}

			var dynamicAssembly = compilerResults.CompiledAssembly;
			var types = dynamicAssembly.GetExportedTypes();
			var dic = new Dictionary<Type, Type>();
			for (int i = 0; i < types.Length; i++) {
				dic.Add(subclasses.ElementAt(i), types[i]);
			}
			return dic;
		}
	}
}
