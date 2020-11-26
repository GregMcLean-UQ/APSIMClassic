using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ModelFramework;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using System.IO;
using ApsimFile;
using CSGeneral;
using System.Reflection;
using System.Xml.Serialization;

[ComVisible(true)]
public class MetDaily
{
    [XmlAnyElement]
    public XmlElement[] Nodes = null;

    [Param(Name = "MetDaily")]
    public XmlNode MetDailyXml = null;

    [Link]
    public SystemComponent MySystem = null;

    string DllFileName;

    [EventHandler]
    public void OnInitialised()
    {
        if (Nodes != null)
        {
            XmlDocument Doc = new XmlDocument();
            MetDailyXml = Doc.AppendChild(Doc.CreateElement("MetDaily"));
            foreach (XmlNode Child in Nodes)
                MetDailyXml.AppendChild(Doc.ImportNode(Child, true));
        }

        DllFileName = Assembly.GetExecutingAssembly().Location;

        // Assembly CompiledAssembly = CompileTextToAssembly();

        // // Go look for our class name.
        // string ScriptClassName = null;
        // // Look for a class called Script
        // Type t = CompiledAssembly.GetType("Script");
        // if (t == null)
        //     throw new Exception("Cannot find a public class called Script");
        // ScriptClassName = "Script";

        // // Create an XML model that we can pass to BuildObjects.
        // XmlDocument NewDoc = new XmlDocument();
        // XmlNode ScriptNode = NewDoc.AppendChild(NewDoc.CreateElement(ScriptClassName));
        // XmlNode ui = XmlHelper.Find(Manager2Xml, "ui");

        // object Model;
        // try
        // {
        //     // Create an instance of the model object.
        //     Model = CompiledAssembly.CreateInstance(ScriptClassName);

        //     // Populate its params from the UI.
        //         if (ui != null)
        //         {
        //             foreach (XmlNode Child in XmlHelper.ChildNodes(ui, ""))
        //             {
        //                 if (XmlHelper.Attribute(Child, "description").Contains("Create child class"))
        //                     ScriptNode.AppendChild(NewDoc.CreateElement(Child.InnerText));

        //                 else if (XmlHelper.Attribute(Child, "type").ToLower() != "category")
        //                     XmlHelper.SetValue(ScriptNode, Child.Name, Child.InnerText);
        //             }
        //         }

        //         foreach (XmlNode Child in XmlHelper.ChildNodes(Manager2Xml, ""))
        //         {
        //             if (Child.Name != "ui" && Child.Name != "Reference" && Child.Name != "text")
        //                 ScriptNode.AppendChild(ScriptNode.OwnerDocument.ImportNode(Child, true));
        //         }
        //         MySystem.AddModel(ScriptNode, CompiledAssembly);

        // }
        // catch (Exception err)
        // {
        //     if (err.InnerException != null)
        //         throw err.InnerException;
        //     else
        //         throw err;
        // }
    }


}
