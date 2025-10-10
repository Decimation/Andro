// Author: Deci | Project: Andro.UI | Name: ViewLocator.cs
// Date: 2025/10/09 @ 22:10:33

using System;
using Andro.UI.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Andro.UI;

public class ViewLocator : IDataTemplate
{

	public Control? Build(object? param)
	{
		if (param is null)
			return null;

		var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
		var type = Type.GetType(name);

		if (type != null) {
			return (Control) Activator.CreateInstance(type)!;
		}

		return new TextBlock { Text = "Not Found: " + name };
	}

	public bool Match(object? data)
	{
		return data is ViewModelBase;
	}

}