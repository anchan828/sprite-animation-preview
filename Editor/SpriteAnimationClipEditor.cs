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
﻿using System.Linq;
using UnityEngine;
using UnityEditor;

namespace SpriteAnimationPreview
{
    [CustomEditor(typeof(AnimationClip)), CanEditMultipleObjects]
    public class SpriteAnimationClipEditor : OverrideEditor
    {
        protected override Editor GetBaseEditor()
        {
            Editor editor = null;
            var baseType = Types.GetType("UnityEditor.AnimationClipEditor", "UnityEditor.dll");
            CreateCachedEditor(targets, baseType, ref editor);
            return editor;
        }

        AnimationClip currentAnimationClip { get { return (AnimationClip)target; } }
        private TextureDrawType selectedTextureDrawType = TextureDrawType.Transparent;
        private SpriteAnimationTimeControl timeControl;
        private Material normalMat;

        void OnEnable()
        {
            normalMat = new Material(Shader.Find("Sprites/Default"));
            timeControl = new SpriteAnimationTimeControl(targets.Cast<AnimationClip>().ToArray());
        }

        void OnDisable()
        {
            timeControl.OnDisable();
            timeControl = null;
            DestroyImmediate(baseEditor);
        }

        public override bool HasPreviewGUI()
        {
            return timeControl.HasSprites(currentAnimationClip) || timeControl.targets.Length < 2;
        }

        public override void OnInspectorGUI()
        {
            if (targets.Length == 1)
                base.OnInspectorGUI();
            else
            {
                var cached = GUI.enabled;
                GUI.enabled = true;
                EditorGUILayout.HelpBox("Multi-object editing not suppoerted.", MessageType.Info);
                GUI.enabled = cached;
            }
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (r.width <= 1f && r.height <= 1f) return;

            var hasSprites = timeControl.HasSprites(currentAnimationClip);
            if (hasSprites)
            {
                var texture = timeControl.GetCurrentPreviewTexture(r, currentAnimationClip);
                switch (selectedTextureDrawType)
                {
                    case TextureDrawType.Normal:
                        EditorGUI.DrawPreviewTexture(r, texture, normalMat, ScaleMode.ScaleToFit);
                        break;
                    case TextureDrawType.Alpha:
                        EditorGUI.DrawTextureAlpha(r, texture, ScaleMode.ScaleToFit);
                        break;
                    case TextureDrawType.Transparent:
                        EditorGUI.DrawTextureTransparent(r, texture, ScaleMode.ScaleToFit);
                        break;
                }
                GUI.Label(r, texture.name, Styles.grayLabel);
            }
            else if (1 < timeControl.targets.Length)
                GUI.Box(r, "No Sprite Data");
            else
                baseEditor.OnInteractivePreviewGUI(r, background);
        }

        public override void OnPreviewSettings()
        {
            var hasSprites = timeControl.HasSprites(currentAnimationClip);

            if (hasSprites)
            {
                DrawTextureDrawType();
                DrawPrevSpriteButton();
                DrawPlayButton();
                DrawNextSpriteButton();
                DrawSpeedSlider();

                if (!timeControl.isPlaying) return;

                foreach (var activeEditor in ActiveEditorTracker.sharedTracker.activeEditors)
                {
                    activeEditor.Repaint();
                }
            }
            else
                baseEditor.OnPreviewSettings();
        }

        private void DrawTextureDrawType()
        {
            selectedTextureDrawType = (TextureDrawType)EditorGUILayout.EnumPopup(selectedTextureDrawType, new GUIStyle("preDropDown"), GUILayout.Width(100));
        }

        private void DrawPlayButton()
        {

            var buttonContent = timeControl.isPlaying ? Contents.pauseButtonContent : Contents.playButtonContent;

            EditorGUI.BeginChangeCheck();

            var isPlaying = GUILayout.Toggle(timeControl.isPlaying, buttonContent, Styles.previewButtonSettingsStyle);

            if (EditorGUI.EndChangeCheck())
            {
                if (isPlaying)
                    timeControl.Play();
                else
                    timeControl.Pause();
            }
        }
        private void DrawPrevSpriteButton()
        {
            if (GUILayout.Button(Contents.prevButtonContent, Styles.previewButtonSettingsStyle))
            {
                timeControl.Pause();
                timeControl.PrevSprite();
            }
        }
        private void DrawNextSpriteButton()
        {
            if (GUILayout.Button(Contents.nextButtonContent, Styles.previewButtonSettingsStyle))
            {
                timeControl.Pause();
                timeControl.NextSprite();
            }
        }

        private void DrawSpeedSlider()
        {

            if (GUILayout.Button(Contents.speedScale, Styles.preLabel)) timeControl.speed = 1;
            timeControl.speed = GUILayout.HorizontalSlider(timeControl.speed, 0, 5, Styles.preSlider, Styles.preSliderThumb);
            GUILayout.Label(timeControl.speed.ToString("0.00"), Styles.preLabel, GUILayout.Width(40));
        }

        class Styles
        {
            public static GUIStyle previewButtonSettingsStyle = new GUIStyle("preButton");
            public static GUIStyle preSlider = new GUIStyle("preSlider");
            public static GUIStyle preSliderThumb = new GUIStyle("preSliderThumb");
            public static GUIStyle preLabel = new GUIStyle("preLabel");
            public static GUIStyle grayLabel = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.gray } };
        }

        class Contents
        {
            public static GUIContent playButtonContent = EditorGUIUtility.IconContent("PlayButton");
            public static GUIContent pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            public static GUIContent prevButtonContent = EditorGUIUtility.IconContent("Animation.PrevKey");
            public static GUIContent nextButtonContent = EditorGUIUtility.IconContent("Animation.NextKey");
            public static GUIContent speedScale = EditorGUIUtility.IconContent("SpeedScale");
        }

        public enum TextureDrawType
        {
            Normal,
            Alpha,
            Transparent
        }
    }


}
