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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace SpriteAnimationPreview
{

    public class SpriteAnimationTimeControl : TimeControl
    {
        public AnimationClip[] targets
        {
            get { return dic.Keys.ToArray(); }
        }

        private Dictionary<AnimationClip, AnimationData> dic;

        public SpriteAnimationTimeControl(params AnimationClip[] animationClips)
        {
            dic = new Dictionary<AnimationClip, AnimationData>(animationClips.Length);
            foreach (var animationClip in animationClips)
                dic.Add(animationClip, new AnimationData(animationClip));
        }

        private float GetStopTime(AnimationClip animationClip)
        {
            var animationClipSettings = AnimationUtility.GetAnimationClipSettings(animationClip);
            return animationClipSettings.stopTime;
        }

        public void OnDisable()
        {
            foreach (var value in dic.Values) value.OnDisable();
        }


        private Sprite GetCurrentSprite(AnimationData d)
        {
            var currentIndex = GetCurrentIndex(d);
            currentIndex += d.offset;
            currentIndex = (int)Mathf.Repeat((float)currentIndex, (float)d.spriteEditors.Count);
            return (Sprite)d.spriteEditors[currentIndex].target;
        }

        private int GetCurrentIndex(AnimationData d)
        {
            return Mathf.FloorToInt(GetCurrentTime(d.startTime, d.stopTime) * d.frameRate);
        }

        public Texture GetCurrentPreviewTexture(Rect previewRect, AnimationClip target)
        {
            var d = GetAnimationData(target);
            var currentSprite = GetCurrentSprite(d);
            return d.GetPreviewTexture(previewRect, currentSprite) ?? AssetPreview.GetAssetPreview(target);
        }

        public void NextSprite()
        {
            foreach (var d in dic.Values) d.offset++;
        }

        public void PrevSprite()
        {
            foreach (var d in dic.Values) d.offset--;
        }

        private class AnimationData
        {
            public List<Editor> spriteEditors { get; private set; }
            public float startTime { get; private set; }
            public float stopTime { get; private set; }
            public int frameRate { get; private set; }
            public int offset { get; set; }

            private Vector2 latestSize;
            private Dictionary<Editor, Texture> latestPreviewTextures;

            public AnimationData(AnimationClip animationClip)
            {
                var animationClipSettings = AnimationUtility.GetAnimationClipSettings(animationClip);

                spriteEditors = GetSpriteEditors(GetSprites(animationClip));
                startTime = animationClipSettings.startTime;
                stopTime = animationClipSettings.stopTime;
                frameRate = Mathf.FloorToInt(animationClip.frameRate);
            }

            private Sprite[] GetSprites(AnimationClip animationClip)
            {
                var sprites = new Sprite[0];

                if (animationClip != null)
                {
                    var editorCurveBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

                    var objectReferenceKeyframes = AnimationUtility.GetObjectReferenceCurve(animationClip,
                        editorCurveBinding);
                    if (objectReferenceKeyframes != null)
                    {
                        sprites = objectReferenceKeyframes
                            .Select(objectReferenceKeyframe => objectReferenceKeyframe.value)
                            .OfType<Sprite>().ToArray();
                    }
                }
                return sprites;
            }

            private List<Editor> GetSpriteEditors(params Sprite[] sprites)
            {
                var type = Types.GetType("UnityEditor.SpriteInspector", "UnityEditor.dll");
                var editors = new List<Editor>();
                foreach (var sprite in sprites)
                {
                    Editor _editor = null;
                    Editor.CreateCachedEditor(sprite, type, ref _editor);
                    if (_editor != null)
                        editors.Add(_editor);
                }

                return editors;
            }

            public Texture GetPreviewTexture(Rect previewRect, Sprite sprite)
            {
                if (IsDirty(previewRect))
                    RebuildPreviewTextures(previewRect);
                var spriteEditor = GetSpriteEditor(sprite);
                return latestPreviewTextures[spriteEditor];
            }

            private void RebuildPreviewTextures(Rect previewRect)
            {
                latestPreviewTextures = new Dictionary<Editor, Texture>(spriteEditors.Capacity);
                latestSize = new Vector2(previewRect.width, previewRect.height);
                for (int i = 0; i < spriteEditors.Count; i++)
                {
                    var editor = spriteEditors[i];
                    var previewTexture = editor.RenderStaticPreview("", null, (int)previewRect.width,
                        (int)previewRect.height);
                    previewTexture.name = string.Format("({1,2}/{2,2}) {0}", editor.target.name, i + 1,
                        spriteEditors.Count);
                    latestPreviewTextures.Add(editor, previewTexture);
                }
            }

            private bool IsDirty(Rect previewRect)
            {
                return !(latestSize.x == previewRect.width && latestSize.y == previewRect.height);
            }

            private Editor GetSpriteEditor(Sprite sprite)
            {
                return spriteEditors.FirstOrDefault(e => e.target == sprite);
            }

            public void OnDisable()
            {
                if (latestPreviewTextures != null)
                    foreach (var key in latestPreviewTextures.Keys)
                    {
                        Object.DestroyImmediate(latestPreviewTextures[key]);
                    }
                if (spriteEditors != null)
                    foreach (var spriteEditor in spriteEditors)
                    {
                        Object.DestroyImmediate(spriteEditor);
                    }
            }
        }
        public bool HasSprites(AnimationClip target)
        {
            var result = false;
            var d = GetAnimationData(target);

            if (d != null)
                result = d.spriteEditors.Any();

            return result;
        }

        private AnimationData GetAnimationData(AnimationClip target)
        {
            return dic.ContainsKey(target) ? dic[target] : null;
        }
    }
}
