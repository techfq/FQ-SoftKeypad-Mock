﻿#pragma checksum "..\..\..\LicenseInput.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "E546D34174838CFE1AA42ACEE12CE709EE000329"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using FQ_KPSoft;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace FQ_KPSoft {
    
    
    /// <summary>
    /// LicenseInput
    /// </summary>
    public partial class LicenseInput : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\LicenseInput.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tb_public_key;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\LicenseInput.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tb_secret_key;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\LicenseInput.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_activate_license;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.5.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/FQ-KPSoft;component/licenseinput.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\LicenseInput.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.5.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.tb_public_key = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.tb_secret_key = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.btn_activate_license = ((System.Windows.Controls.Button)(target));
            
            #line 16 "..\..\..\LicenseInput.xaml"
            this.btn_activate_license.Click += new System.Windows.RoutedEventHandler(this.btn_activate_license_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 18 "..\..\..\LicenseInput.xaml"
            ((System.Windows.Documents.Hyperlink)(target)).RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
