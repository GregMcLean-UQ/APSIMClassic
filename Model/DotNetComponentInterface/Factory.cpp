#include "Factory.h"
#include "ApsimComponent.h"

Factory::Factory() 
   {
   // --------------------------------------------------------------------
   // Constructor
   // --------------------------------------------------------------------
   
   RegisteredProperties = gcnew List<FactoryProperty^>();
   RegisteredEventHandlers = gcnew List<EvntHandler^>();
   RegisteredEvents = gcnew List<FactoryEvent^>();
   Links = gcnew List<LinkField^>();
   }

void Factory::Create(String^ Xml, 
                     Assembly^ AssemblyWithTypes,
                     ModelFramework::ApsimComponent^ ParentComponent)
   {
   // --------------------------------------------------------------------
   // Create instances (and populate their fields and properies of all 
   // classes as specified by the Xml passed in. The newly created root
   // instance can be retrieved by the 'Root' property.
   // --------------------------------------------------------------------
   CallingAssembly = AssemblyWithTypes;
   XmlDocument^ Doc = gcnew XmlDocument();
   Doc->LoadXml(Xml);
   RemoveShortCuts(Doc->DocumentElement);
   _Root = CreateInstance(Doc->DocumentElement, nullptr, nullptr, ParentComponent);
   }

/// <summary>
/// Resolve all links and initialise all Instances
/// </summary>
void Factory::Initialise()
   {
   ResolveLinks();
   CallInitialisedOnAll(_Root);
   }

//void Factory::Create(XmlNode^ Node, Assembly^ AssemblyWithTypes)
//   {
//   // --------------------------------------------------------------------
//   // Create instances (and populate their fields and properies of all 
//   // classes as specified by the Xml passed in. The newly created root
//   // instance can be retrieved by the 'Root' property.
//   // --------------------------------------------------------------------
//   CallingAssembly = AssemblyWithTypes;
//   _Root = CreateInstance(Node, nullptr, nullptr);
//   }
Instance^ Factory::CreateInstance(XmlNode^ Node, 
                                  XmlNode^ Parent, 
                                  Instance^ ParentInstance, 
                                  ModelFramework::ApsimComponent^ ParentComponent)
   {
   // --------------------------------------------------------------------
   // Create an instance of a the 'Instance' class based on the 
   // Node type passed in information. Then populate the instance based
   // on the child XML nodes.
   // --------------------------------------------------------------------
   Type^ ClassType = GetTypeOfChild(Node, ParentInstance);
   if (ClassType == nullptr)
      throw gcnew Exception("Cannot find a class called: " + Node->Name);
   Instance^ CreatedInstance = dynamic_cast<Instance^> (Activator::CreateInstance(ClassType));
   DerivedInstance^ DerivativeInstance = dynamic_cast<DerivedInstance^> (CreatedInstance);
   if (DerivativeInstance)
   {
      DerivativeInstance->Initialise(Node, ParentInstance, ParentComponent);
   }
   else if (CreatedInstance)
   {
      CreatedInstance->Initialise(XmlHelper::Name(Node), ParentInstance, ParentComponent);
	   GetAllProperties(CreatedInstance, Parent);
      GetAllEventHandlers(CreatedInstance);
      GetAllEvents(CreatedInstance);
      PopulateParams(CreatedInstance, Node, ParentComponent);
      
   }
   else
   {
	   throw gcnew Exception("Class " + Node->Name + " must be derived from the \"Instance\" class");
   }

   return CreatedInstance;
   }

void Factory::GetAllProperties(Instance^ Obj, XmlNode^ Parent)
   {
   // --------------------------------------------------------------------
   // Go through all reflected fields and properties that are tagged
   // with a 'Param' or 'Input' attribute and add them to our list
   // of registered properties.
   // --------------------------------------------------------------------
   for each (FieldInfo^ Property in Obj->GetType()->GetFields(BindingFlags::FlattenHierarchy | BindingFlags::Instance  | BindingFlags::Static | BindingFlags::Public | BindingFlags::NonPublic))
      {
      bool AddProperty = false;
      bool IsOutput = false;
      array<Object^>^ Attributes = Property->GetCustomAttributes(false);
      for each (Object^ Attr in Attributes)
         {
         IsOutput = (IsOutput || dynamic_cast<Output^>(Attr) != nullptr);
         if (dynamic_cast<Param^>(Attr) != nullptr || 
             dynamic_cast<Input^>(Attr) != nullptr || 
             dynamic_cast<Output^>(Attr) != nullptr)
             AddProperty = true;
         if (dynamic_cast<Link^>(Attr) != nullptr)
            {
            LinkField^ LinkF = gcnew LinkField(Obj, Property, dynamic_cast<Link^> (Attr));
            Links->Add(LinkF);
            }

         }
      if (AddProperty)
         {
         FactoryProperty^ NewProperty = gcnew FactoryProperty(gcnew ReflectedField(Property, Obj), Parent);
         if (IsOutput)
            RemoveRegisteredOutput(NewProperty->OutputName);
         RegisteredProperties->Add(NewProperty);
         }
      }
   for each (PropertyInfo^ Property in Obj->GetType()->GetProperties(BindingFlags::FlattenHierarchy | BindingFlags::Instance | BindingFlags::Public | BindingFlags::NonPublic))
      {
      bool AddProperty = false;
      bool IsOutput = false;
      array<Object^>^ Attributes = Property->GetCustomAttributes(false);
      for each (Object^ Attr in Attributes)
         {
         IsOutput = (IsOutput || dynamic_cast<Output^>(Attr) != nullptr);
         if (dynamic_cast<Param^>(Attr) != nullptr || 
             dynamic_cast<Input^>(Attr) != nullptr || 
             dynamic_cast<Output^>(Attr) != nullptr)
            AddProperty = true;
         }
      if (AddProperty)
         {
         FactoryProperty^ NewProperty = gcnew FactoryProperty(gcnew ReflectedProperty(Property, Obj), Parent);
         if (IsOutput)
            RemoveRegisteredOutput(NewProperty->OutputName);
         RegisteredProperties->Add(NewProperty);
         }
      }
   }
void Factory::RemoveRegisteredOutput(String^ OutputName)
   {
   // --------------------------------------------------------------------
   // Remove the specified [output] from the list of registered properties.
   // Duplicates can happen when an [output] in a base class is
   // overridden in a derived class [output]. In this case we want the last
   // duplicate and it superseeds previous ones (base classes)
   // --------------------------------------------------------------------
   for (int i = 0; i != RegisteredProperties->Count; i++)
      {
      if (RegisteredProperties[i]->IsOutput && 
          String::Compare(RegisteredProperties[i]->OutputName, OutputName) == 0)
         {
         RegisteredProperties->RemoveAt(i);
         return;
         }
      }
   }   
void Factory::GetAllEventHandlers(Instance^ Obj)
   {
   // --------------------------------------------------------------------
   // Goes through the model looking for all event handlers that start
   // with 'On' and are marked 'EventHandler'
   // --------------------------------------------------------------------
   
   for each (MethodInfo^ Method in Obj->GetType()->GetMethods(BindingFlags::Instance | BindingFlags::Public | BindingFlags::NonPublic))
      {
      array<Object^>^ Attributes = Method->GetCustomAttributes(false);
      for each (Object^ Attr in Attributes)
         {
         if (dynamic_cast<::EventHandler^>(Attr) != nullptr && 
             Method->Name->Length > 2 &&
             Method->Name->Substring(0, 2) == "On")
            RegisteredEventHandlers->Add(gcnew FactoryEventHandler(Method, Obj));
         }
      }
   }
void Factory::GetAllEvents(Instance^ Obj)
   {
   // --------------------------------------------------------------------
   // Goes through the model looking for all events
   // --------------------------------------------------------------------
   for each (EventInfo^ Event in Obj->GetType()->GetEvents(BindingFlags::Instance | BindingFlags::Public | BindingFlags::NonPublic))
      {
      array<Object^>^ Attributes = Event->GetCustomAttributes(false);
      for each (Object^ Attr in Attributes)
         {
            if (dynamic_cast<::Event^>(Attr) != nullptr)
              RegisteredEvents->Add(gcnew FactoryEvent(Event, Obj));
         }
      }
   }   

   void Factory::PopulateParams(Instance^ Obj, XmlNode^ Node,ModelFramework::ApsimComponent^ ParentComponent)
      {
      // Look for an XmlNode param. If found then given it our current 'Node'.
      bool HavePassedXMLToObj = false;
      for each (FactoryProperty^ Property in RegisteredProperties)
         {
         if (Property->TypeName == "XmlNode" && Property->OutputName->IndexOf(Node->Name) == 0)
            {
            Property->SetObject(Node);
            HavePassedXMLToObj = true;
            }
         }

      // Go through all child XML nodes for the node passed in and set
      // the corresponding property values in the Obj instance passed in.
      if (!HavePassedXMLToObj)
         {
         for each (XmlNode^ Child in Node->ChildNodes)
            {
            if (dynamic_cast<XmlComment^>(Child) == nullptr)
               {
               Type^ t = GetTypeOfChild(Child, Obj);
               if (t != nullptr && t->IsSubclassOf(Instance::typeid))
                  {
                  // Create a child instance - indirect recursion.
                  Instance^ ChildInstance = CreateInstance(Child, Child, Obj, ParentComponent);
                  Obj->Add(ChildInstance);   

                  FactoryProperty^ Parameter = FindProperty(Child);
                  if (XmlHelper::Name(Child)->Contains("["))
                     {
                     String^ ArrayName = XmlHelper::Name(Child);
                     StringManip::SplitOffBracketedValue(ArrayName, '[', ']');
                     XmlHelper::SetName(Child, ArrayName);
                     Parameter = FindProperty(Child);
                     if (Parameter == nullptr)
                        throw gcnew Exception("Cannot find array: " + ArrayName);
                     Parameter->AddToList(ChildInstance);
                     }

                  else if (Parameter != nullptr && Parameter->IsParam && !Parameter->TypeName->Contains("System::"))
                     Parameter->SetObject(ChildInstance);
                  }
               else if (Child->Name == "Memo")
                  {
                  // Ignore memo fields.
                  }
               else if (!Child->HasChildNodes && Child->InnerText == "")
                  throw gcnew Exception("Cannot have a blank value for property: " + Child->Name);
               else if (Child->HasChildNodes)
                  {
                  FactoryProperty^ Parameter = FindProperty(Child);                  
                  if (Parameter == nullptr)
                     throw gcnew Exception("Cannot set value of property: " + Child->Name + " in object: " + Obj->InstanceName + ". The property must have either a [Param] or [Input] attribute.");
                  else
                     {
                     Parameter->Name = XmlHelper::Name(Child);
                     Parameter->Set(Child);
                     }
                  }
               }
            }
         }
      }

   Type^ Factory::GetTypeOfChild(XmlNode^ Child, Instance^ Obj)
      {
      FactoryProperty^ Parameter = FindProperty(Child);
      Type^ t = CallingAssembly->GetType(Child->Name);
      if (t == nullptr && Parameter != nullptr)
         t = CallingAssembly->GetType(Parameter->TypeName);
      if (t == nullptr && Parameter != nullptr)
         t = CallingAssembly->GetType(Obj->Name + "+" + Parameter->TypeName);
      return t;
      }

   FactoryProperty^ Factory::FindProperty(XmlNode^ Child)
      {
      // --------------------------------------------------------------------
      // Go through all our registered properties and look for the one that
      // has the specified name. Returns null if not found.
      // --------------------------------------------------------------------
      String^ FQN = CalcParentName(Child);
      for each (FactoryProperty^ Property in RegisteredProperties)
         {
         if (Property->FQN->ToLower() == FQN->ToLower())
            return Property;
         }

      // Go look for the plural version - property might be an array.
      FQN = FQN + "s";
      for each (FactoryProperty^ Property in RegisteredProperties)
         {
         if (Property->FQN->ToLower() == FQN->ToLower())
            return Property;
         }

      return nullptr;
      }
    
template<typename T> void CheckArray(FactoryProperty^ Property, String^ ErrorString)
{
   array<T>^ arrayObj = (array<T>^)Property->Get;
   for (int i = 0; i < arrayObj->Length; i++)
   {
     double val = arrayObj[i];
     if (!Double::IsNaN(Property->ParamMinVal) && 
         val < Property->ParamMinVal)
        ErrorString += "The value provided for element " + i +
		               " of parameter " + Property->FQN +
			           " is less than its allowed minimum (" + 
			           val + " vs. " +
			           Property->ParamMinVal + ")\n";
   	 if (!Double::IsNaN(Property->ParamMaxVal) && 
		 val > Property->ParamMaxVal)
   	    ErrorString += "The value provided for element " + i +
		                " of parameter " + Property->FQN +
			            " is greater than its allowed maximum (" + 
				        val + " vs. " +
				        Property->ParamMaxVal + ")\n";
   }
}

void Factory::CheckParameters()
   {
	// -----------------------------------------------
	// Check for parameters in the model that
	// haven't been given a value and throw if any 
	// are found.
    // Also do range checking, if applicable.
	// -----------------------------------------------
	
	String^ Errors = "";
	String^ RangeErrors = "";
	for (int i = 0; i != RegisteredProperties->Count; i++)
	{
	   FactoryProperty^ Property = RegisteredProperties[i];
	   if (Property->IsParam)
	   {
		  if (!Property->HasAsValue && !Property->OptionalParam)
	      {
	         if (Errors != "")
	            Errors += ", ";
	         Errors += Property->FQN;
          }
		  // Is there a tidier way to do this?
		  if (Property->HasAsValue && 
			 (!Double::IsNaN(Property->ParamMinVal) || 
			  !Double::IsNaN(Property->ParamMaxVal)))
		  {
             if (Property->TypeName == "Double" ||
			     Property->TypeName == "Single" ||
			     Property->TypeName == "Int32")
     	     {
			    double val = Convert::ToDouble(Property->Get->ToString());
   			    if (!Double::IsNaN(Property->ParamMinVal) && 
				    val < Property->ParamMinVal)
				    RangeErrors += "The value provided for parameter " + Property->FQN +
			         " is less than its allowed minimum (" + 
				     Property->Get->ToString() + " vs. " +
				     Property->ParamMinVal + ")\n";
   			    if (!Double::IsNaN(Property->ParamMaxVal) && 
				    val > Property->ParamMaxVal)
   			      RangeErrors += "The value provided for parameter " + Property->FQN +
			         " is greater than its allowed maximum (" + 
				     Property->Get->ToString() + " vs. " +
				     Property->ParamMaxVal + ")\n";
			 }
             else if (Property->TypeName == "Double[]")
			    CheckArray<double>(Property, RangeErrors);
             else if (Property->TypeName == "Single[]")
			    CheckArray<float>(Property, RangeErrors);
             else if (Property->TypeName == "Int32[]")
			    CheckArray<int>(Property, RangeErrors);
		  }
	   } 
   }
   if (Errors != "")
      throw gcnew Exception("The following parameters haven't been initialised: " + Errors);
   if (RangeErrors != "")
	   throw gcnew Exception("In " + Root->InstanceName + ", the following parameters are outside their allowable ranges:\n" + RangeErrors);
	}
      
void Factory::RemoveShortCuts(XmlNode^ Node)
   {
   // -----------------------------------------------
   // Remove any shortcut nodes in the children of
   // the specified node.
	// -----------------------------------------------
	
   String^ ShortCutPath = XmlHelper::Attribute(Node, "shortcut");
   if (ShortCutPath != "")
      {
      // Shortcut strings will be a full path e.g. /FrenchBean/Model/Plant/Phenology/ThermalTime
      // But our Node->OwnerDocument->DocumentElemen is <Plant> 
      // So we need to find /Plant/ and remove everything on the path before that. This way
      // we'll end up with a relative path e.g. Plant/Phenology/ThermalTime
      int PosPlant = ShortCutPath->IndexOf("/Plant/");
      if (PosPlant == -1)
         throw gcnew Exception("Invalid shortcut path: " + ShortCutPath);
      ShortCutPath = ShortCutPath->Remove(0, PosPlant+7); // Get rid of the /Plant/ string.
         
      XmlNode^ ReferencedNode = XmlHelper::Find(Node->OwnerDocument->DocumentElement, ShortCutPath);
      if (ReferencedNode == nullptr)
         throw gcnew Exception("Cannot find short cut node: " + ShortCutPath);
      XmlNode^ NewNode = ReferencedNode->CloneNode(true);
      Node->ParentNode->ReplaceChild(NewNode, Node);
      if (XmlHelper::Attribute(Node, "name") != "")
         XmlHelper::SetName(NewNode, XmlHelper::Name(Node));
      }
	
	for (int i = 0; i < Node->ChildNodes->Count; i++)
	   RemoveShortCuts(Node->ChildNodes[i]);
   }
      

void Factory::ResolveLinks()
   {
   // -----------------------------------------------
   // Resolve all [Links].
   // -----------------------------------------------

	for (int i = 0; i < Links->Count; i++)
      Links[i]->Resolve();

   }

/// <summary>
/// Do a depth first walk of the Instance tree, calling each Instance's Initialised method.
/// </summary>
void Factory::CallInitialisedOnAll(Instance^ Obj)
   {
   for each (Instance^ Child in Obj->Children)
      CallInitialisedOnAll(Child);

   Obj->Initialised();
   }
