﻿#pragma checksum "..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "CA9DED856273ED5AA23E466CDC0F2BB9E0A28CDC"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using GUI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
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


namespace GUI {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 87 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button FindButton;
        
        #line default
        #line hidden
        
        
        #line 88 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button UploadFilesToRepo;
        
        #line default
        #line hidden
        
        
        #line 89 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BuildButton;
        
        #line default
        #line hidden
        
        
        #line 90 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AddTestElementButton;
        
        #line default
        #line hidden
        
        
        #line 91 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ShutDownPoolProcessButton;
        
        #line default
        #line hidden
        
        
        #line 92 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SendMainBuilderButton;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SendChildBuilderButton;
        
        #line default
        #line hidden
        
        
        #line 94 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SendBuildRequestToRepo;
        
        #line default
        #line hidden
        
        
        #line 95 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox listBox1;
        
        #line default
        #line hidden
        
        
        #line 96 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox listBox2;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox listBox3;
        
        #line default
        #line hidden
        
        
        #line 98 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock textBlock1;
        
        #line default
        #line hidden
        
        
        #line 100 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox textBox1;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/GUI;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 60 "..\..\MainWindow.xaml"
            ((GUI.MainWindow)(target)).Loaded += new System.Windows.RoutedEventHandler(this.OnLoad);
            
            #line default
            #line hidden
            
            #line 61 "..\..\MainWindow.xaml"
            ((GUI.MainWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.OnClosing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.FindButton = ((System.Windows.Controls.Button)(target));
            
            #line 87 "..\..\MainWindow.xaml"
            this.FindButton.Click += new System.Windows.RoutedEventHandler(this.FindButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.UploadFilesToRepo = ((System.Windows.Controls.Button)(target));
            
            #line 88 "..\..\MainWindow.xaml"
            this.UploadFilesToRepo.Click += new System.Windows.RoutedEventHandler(this.UploadFilesToRepo_OnClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.BuildButton = ((System.Windows.Controls.Button)(target));
            
            #line 89 "..\..\MainWindow.xaml"
            this.BuildButton.Click += new System.Windows.RoutedEventHandler(this.BuildButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.AddTestElementButton = ((System.Windows.Controls.Button)(target));
            
            #line 90 "..\..\MainWindow.xaml"
            this.AddTestElementButton.Click += new System.Windows.RoutedEventHandler(this.AddTestElementButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ShutDownPoolProcessButton = ((System.Windows.Controls.Button)(target));
            
            #line 91 "..\..\MainWindow.xaml"
            this.ShutDownPoolProcessButton.Click += new System.Windows.RoutedEventHandler(this.ShutDownPoolProcessButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 7:
            this.SendMainBuilderButton = ((System.Windows.Controls.Button)(target));
            
            #line 92 "..\..\MainWindow.xaml"
            this.SendMainBuilderButton.Click += new System.Windows.RoutedEventHandler(this.SendMainBuilderButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 8:
            this.SendChildBuilderButton = ((System.Windows.Controls.Button)(target));
            
            #line 93 "..\..\MainWindow.xaml"
            this.SendChildBuilderButton.Click += new System.Windows.RoutedEventHandler(this.SendChildBuilderButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 9:
            this.SendBuildRequestToRepo = ((System.Windows.Controls.Button)(target));
            
            #line 94 "..\..\MainWindow.xaml"
            this.SendBuildRequestToRepo.Click += new System.Windows.RoutedEventHandler(this.SendBuildRequestToRepo_OnClick);
            
            #line default
            #line hidden
            return;
            case 10:
            this.listBox1 = ((System.Windows.Controls.ListBox)(target));
            
            #line 95 "..\..\MainWindow.xaml"
            this.listBox1.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(this.ListBox1_OnPreviewMouseDown);
            
            #line default
            #line hidden
            return;
            case 11:
            this.listBox2 = ((System.Windows.Controls.ListBox)(target));
            
            #line 96 "..\..\MainWindow.xaml"
            this.listBox2.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(this.ListBox2_OnPreviewMouseDown);
            
            #line default
            #line hidden
            return;
            case 12:
            this.listBox3 = ((System.Windows.Controls.ListBox)(target));
            return;
            case 13:
            this.textBlock1 = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 14:
            this.textBox1 = ((System.Windows.Controls.TextBox)(target));
            
            #line 100 "..\..\MainWindow.xaml"
            this.textBox1.TextInput += new System.Windows.Input.TextCompositionEventHandler(this.TextBox1_OnTextInput);
            
            #line default
            #line hidden
            
            #line 100 "..\..\MainWindow.xaml"
            this.textBox1.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.TextBox1_OnTextChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

