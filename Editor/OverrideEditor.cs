/*
The MIT License (MIT)

Copyright (c) 2015 kyusyukeigo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
﻿using UnityEngine;
using System.Reflection;
using UnityEditor;

namespace SpriteAnimationPreview
{
    public abstract class OverrideEditor : Editor
    {
        readonly MethodInfo methodInfo = typeof(Editor).GetMethod("OnHeaderGUI", BindingFlags.NonPublic | BindingFlags.Instance);
        private Editor m_BaseEditor;

        protected Editor baseEditor
        {
            get { return m_BaseEditor ?? (m_BaseEditor = GetBaseEditor()); }
            set { m_BaseEditor = value; }
        }

        protected abstract Editor GetBaseEditor();

        public override void OnInspectorGUI()
        {
            baseEditor.OnInspectorGUI();
        }

        public override string GetInfoString()
        {
            return baseEditor.GetInfoString();
        }

        public override void OnPreviewSettings()
        {
            baseEditor.OnPreviewSettings();
        }

        public override void ReloadPreviewInstances()
        {
            baseEditor.ReloadPreviewInstances();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            return baseEditor.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        protected override void OnHeaderGUI()
        {
            methodInfo.Invoke(baseEditor, new object[0]);
        }

        public override bool RequiresConstantRepaint()
        {
            return baseEditor.RequiresConstantRepaint();
        }

        public override bool UseDefaultMargins()
        {
            return baseEditor.UseDefaultMargins();
        }

        public override GUIContent GetPreviewTitle()
        {
            return baseEditor.GetPreviewTitle();
        }
    }
}
