﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;


public class EventPublisher
{
    private EventInfo Info;
    private object TheModel;
    public EventPublisher(EventInfo I, object Model)
    {
        Info = I;
        TheModel = Model;
    }
    public string Name
    {
        get
        {
            return Info.Name;
        }
    }

    public void ConnectTo(EventSubscriber Subscriber)
    {
        Delegate D = Delegate.CreateDelegate(Info.EventHandlerType, Subscriber.TheModel, Subscriber.Info);
        Info.AddEventHandler(TheModel, D);
        Subscriber.Publisher = this;
        Subscriber.D.Add(D);
    }


    internal void DisconnectTo(EventSubscriber Subscriber)
    {
        Delegate[] DL = Subscriber.D[0].GetInvocationList();
        Info.RemoveEventHandler(TheModel, Subscriber.D[0]);
        
    }
}

public class EventSubscriber
{
    internal MethodInfo Info;
    internal object TheModel;
    private string _Name;
    internal EventPublisher Publisher;
    internal List<Delegate> D = new List<Delegate>();
    public bool _Enabled = true;

    public EventSubscriber(string Name, MethodInfo I, object Model)
    {
        if (Name != null)
            _Name = Name;
        Info = I;
        TheModel = Model;
    }
    public string Name
    {
        get
        {
            if (_Name != null)
                return _Name;
            string FullName = Info.Name;
            if (FullName.Substring(0, 2) == "On")
                FullName = FullName.Remove(0, 2);
            return FullName;
        }
    }
    public void Invoke()
    {
        if (Info != null && Enabled)
            Info.Invoke(TheModel, null);
    }

    public void Connect()
    {
        if (Publisher != null)
            Publisher.ConnectTo(this);
    }

    public void Disconnect()
    {
        if (Publisher != null)
            Publisher.DisconnectTo(this);
    }

    public bool Enabled
    {
        get
        {
            return _Enabled;
        }
        set
        {
            if (_Enabled != value)
            {
                _Enabled = value;
                if (Publisher != null)
                {
                    if (Enabled)
                        Publisher.ConnectTo(this);
                    else
                        Publisher.DisconnectTo(this);
                }
            }
        }
    }


}