﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Traffic_control.ServiceReference1 {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="BlockObjectContract", Namespace="http://schemas.datacontract.org/2004/07/Simulator.Simulation.WCFInterfaces")]
    [System.SerializableAttribute()]
    public partial class BlockObjectContract : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int GIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double RotationField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double XField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double YField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int GID {
            get {
                return this.GIDField;
            }
            set {
                if ((this.GIDField.Equals(value) != true)) {
                    this.GIDField = value;
                    this.RaisePropertyChanged("GID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double Rotation {
            get {
                return this.RotationField;
            }
            set {
                if ((this.RotationField.Equals(value) != true)) {
                    this.RotationField = value;
                    this.RaisePropertyChanged("Rotation");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double X {
            get {
                return this.XField;
            }
            set {
                if ((this.XField.Equals(value) != true)) {
                    this.XField = value;
                    this.RaisePropertyChanged("X");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double Y {
            get {
                return this.YField;
            }
            set {
                if ((this.YField.Equals(value) != true)) {
                    this.YField = value;
                    this.RaisePropertyChanged("Y");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TrafficLightGroupContract", Namespace="http://schemas.datacontract.org/2004/07/Simulator.Simulation.WCFInterfaces")]
    [System.SerializableAttribute()]
    public partial class TrafficLightGroupContract : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Traffic_control.ServiceReference1.TrafficLightContract[] TrafficLightsField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ID {
            get {
                return this.IDField;
            }
            set {
                if ((this.IDField.Equals(value) != true)) {
                    this.IDField = value;
                    this.RaisePropertyChanged("ID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Traffic_control.ServiceReference1.TrafficLightContract[] TrafficLights {
            get {
                return this.TrafficLightsField;
            }
            set {
                if ((object.ReferenceEquals(this.TrafficLightsField, value) != true)) {
                    this.TrafficLightsField = value;
                    this.RaisePropertyChanged("TrafficLights");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TrafficLightContract", Namespace="http://schemas.datacontract.org/2004/07/Simulator.Simulation.WCFInterfaces")]
    [System.SerializableAttribute()]
    public partial class TrafficLightContract : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Traffic_control.ServiceReference1.TrafficLightDirection DirectionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Traffic_control.ServiceReference1.TrafficLightContract NeighborField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double PosXField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double PosYField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Traffic_control.ServiceReference1.TrafficLightStatus StatusField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Traffic_control.ServiceReference1.TrafficLightDirection Direction {
            get {
                return this.DirectionField;
            }
            set {
                if ((this.DirectionField.Equals(value) != true)) {
                    this.DirectionField = value;
                    this.RaisePropertyChanged("Direction");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ID {
            get {
                return this.IDField;
            }
            set {
                if ((this.IDField.Equals(value) != true)) {
                    this.IDField = value;
                    this.RaisePropertyChanged("ID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Traffic_control.ServiceReference1.TrafficLightContract Neighbor {
            get {
                return this.NeighborField;
            }
            set {
                if ((object.ReferenceEquals(this.NeighborField, value) != true)) {
                    this.NeighborField = value;
                    this.RaisePropertyChanged("Neighbor");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double PosX {
            get {
                return this.PosXField;
            }
            set {
                if ((this.PosXField.Equals(value) != true)) {
                    this.PosXField = value;
                    this.RaisePropertyChanged("PosX");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double PosY {
            get {
                return this.PosYField;
            }
            set {
                if ((this.PosYField.Equals(value) != true)) {
                    this.PosYField = value;
                    this.RaisePropertyChanged("PosY");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Traffic_control.ServiceReference1.TrafficLightStatus Status {
            get {
                return this.StatusField;
            }
            set {
                if ((this.StatusField.Equals(value) != true)) {
                    this.StatusField = value;
                    this.RaisePropertyChanged("Status");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TrafficLightDirection", Namespace="http://schemas.datacontract.org/2004/07/Simulator.Simulation.WCFInterfaces")]
    public enum TrafficLightDirection : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Top = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Bottom = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Left = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Right = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TrafficLightStatus", Namespace="http://schemas.datacontract.org/2004/07/Simulator.Simulation.WCFInterfaces")]
    public enum TrafficLightStatus : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Red = 12,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Yellow = 13,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Green = 14,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        YellowRed = 15,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.SimulatorServiceMap")]
    public interface SimulatorServiceMap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SimulatorServiceMap/GetMap", ReplyAction="http://tempuri.org/SimulatorServiceMap/GetMapResponse")]
        string GetMap();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SimulatorServiceMap/GetMap", ReplyAction="http://tempuri.org/SimulatorServiceMap/GetMapResponse")]
        System.Threading.Tasks.Task<string> GetMapAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SimulatorServiceMap/GetDynamicObjects", ReplyAction="http://tempuri.org/SimulatorServiceMap/GetDynamicObjectsResponse")]
        Traffic_control.ServiceReference1.BlockObjectContract[] GetDynamicObjects();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SimulatorServiceMap/GetDynamicObjects", ReplyAction="http://tempuri.org/SimulatorServiceMap/GetDynamicObjectsResponse")]
        System.Threading.Tasks.Task<Traffic_control.ServiceReference1.BlockObjectContract[]> GetDynamicObjectsAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface SimulatorServiceMapChannel : Traffic_control.ServiceReference1.SimulatorServiceMap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SimulatorServiceMapClient : System.ServiceModel.ClientBase<Traffic_control.ServiceReference1.SimulatorServiceMap>, Traffic_control.ServiceReference1.SimulatorServiceMap {
        
        public SimulatorServiceMapClient() {
        }
        
        public SimulatorServiceMapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public SimulatorServiceMapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SimulatorServiceMapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SimulatorServiceMapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetMap() {
            return base.Channel.GetMap();
        }
        
        public System.Threading.Tasks.Task<string> GetMapAsync() {
            return base.Channel.GetMapAsync();
        }
        
        public Traffic_control.ServiceReference1.BlockObjectContract[] GetDynamicObjects() {
            return base.Channel.GetDynamicObjects();
        }
        
        public System.Threading.Tasks.Task<Traffic_control.ServiceReference1.BlockObjectContract[]> GetDynamicObjectsAsync() {
            return base.Channel.GetDynamicObjectsAsync();
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.SimulatorServiceTrafficControl")]
    public interface SimulatorServiceTrafficControl {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SimulatorServiceTrafficControl/GetTrafficInitData", ReplyAction="http://tempuri.org/SimulatorServiceTrafficControl/GetTrafficInitDataResponse")]
        string GetTrafficInitData();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SimulatorServiceTrafficControl/GetTrafficInitData", ReplyAction="http://tempuri.org/SimulatorServiceTrafficControl/GetTrafficInitDataResponse")]
        System.Threading.Tasks.Task<string> GetTrafficInitDataAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SimulatorServiceTrafficControl/GetTrafficLightGroups", ReplyAction="http://tempuri.org/SimulatorServiceTrafficControl/GetTrafficLightGroupsResponse")]
        Traffic_control.ServiceReference1.TrafficLightGroupContract[] GetTrafficLightGroups();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SimulatorServiceTrafficControl/GetTrafficLightGroups", ReplyAction="http://tempuri.org/SimulatorServiceTrafficControl/GetTrafficLightGroupsResponse")]
        System.Threading.Tasks.Task<Traffic_control.ServiceReference1.TrafficLightGroupContract[]> GetTrafficLightGroupsAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface SimulatorServiceTrafficControlChannel : Traffic_control.ServiceReference1.SimulatorServiceTrafficControl, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SimulatorServiceTrafficControlClient : System.ServiceModel.ClientBase<Traffic_control.ServiceReference1.SimulatorServiceTrafficControl>, Traffic_control.ServiceReference1.SimulatorServiceTrafficControl {
        
        public SimulatorServiceTrafficControlClient() {
        }
        
        public SimulatorServiceTrafficControlClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public SimulatorServiceTrafficControlClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SimulatorServiceTrafficControlClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SimulatorServiceTrafficControlClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetTrafficInitData() {
            return base.Channel.GetTrafficInitData();
        }
        
        public System.Threading.Tasks.Task<string> GetTrafficInitDataAsync() {
            return base.Channel.GetTrafficInitDataAsync();
        }
        
        public Traffic_control.ServiceReference1.TrafficLightGroupContract[] GetTrafficLightGroups() {
            return base.Channel.GetTrafficLightGroups();
        }
        
        public System.Threading.Tasks.Task<Traffic_control.ServiceReference1.TrafficLightGroupContract[]> GetTrafficLightGroupsAsync() {
            return base.Channel.GetTrafficLightGroupsAsync();
        }
    }
}
