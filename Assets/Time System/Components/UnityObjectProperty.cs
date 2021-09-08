using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

using MB;
using System.Reflection;

namespace MB
{
    [Serializable]
    public class UnityObjectProperty
    {
        [SerializeField]
        Object target = default;
        public Object Target => target;

        [SerializeField]
        string name = default;
        public string Name => name;

        public const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public PropertyInfo LoadProperty() => LoadProperty(target, name);

        //Static Utility

        public static PropertyInfo LoadProperty(Object target, string name)
        {
            var type = target.GetType();

            var property = type.GetProperty(name, Flags);

            return property;
        }

        public static string FormatEntryName(Object target, string name)
        {
            if (target == null || name == "")
                return "Unassigned";
            else
                return $"{target.GetType().Name} -> {name}";
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(UnityObjectProperty))]
        public class Drawer : PropertyDrawer
        {
            SerializedProperty GetTargetProperty(SerializedProperty property) => property.FindPropertyRelative("target");
            SerializedProperty GetNameProperty(SerializedProperty property) => property.FindPropertyRelative("name");

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
            {
                var target = GetTargetProperty(property);
                var name = GetNameProperty(property);

                var gameObject = (property.serializedObject.targetObject as Component).gameObject;

                DrawLabel(ref rect, label);
                DrawDropdown(ref rect, target, name, gameObject);
            }

            void DrawLabel(ref Rect rect, GUIContent label)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            void DrawDropdown(ref Rect rect, SerializedProperty target, SerializedProperty name, GameObject gameObject)
            {
                var label = FormatEntryName(target.objectReferenceValue, name.stringValue);
                var content = new GUIContent($" {label}");

                if (EditorGUI.DropdownButton(rect, content, FocusType.Passive))
                {
                    var selection = new Entry(target.objectReferenceValue, name.stringValue);
                    Dropdown.Show(rect, gameObject, selection, Handler);

                    void Handler(Entry entry)
                    {
                        target.objectReferenceValue = entry.Target;
                        name.stringValue = entry.Name;

                        target.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        public static class Dropdown
        {
            public delegate void HandlerDelegate(Entry entry);

            public static void Show(Rect rect, GameObject gameObject, Entry selection, HandlerDelegate handler)
            {
                var menu = new GenericMenu();

                Register(menu, gameObject, selection, Callback);

                var components = gameObject.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                    Register(menu, components[i], selection, Callback);

                menu.DropDown(rect);

                void Callback(object value)
                {
                    var selection = (Entry)value;

                    handler.Invoke(selection);
                }
            }

            static void Register(GenericMenu menu, Object target, Entry selection, GenericMenu.MenuFunction2 callback)
            {
                var type = target.GetType();

                var properties = TypeAnalyzer.Iterate(type);
                var lookup = properties.ToLookup(x => x.PropertyType).OrderBy(x => x.Key.Name);

                foreach (var item in lookup)
                {
                    var path = $"{type.Name}/{item.Key.Name}/";

                    var values = item.OrderBy(x => x.Name);
                    foreach (var property in values)
                    {
                        var entry = new Entry(target, property.Name);

                        var content = new GUIContent(path + property.Name);
                        var isOn = entry == selection;

                        menu.AddItem(content, isOn, callback, entry);
                    }
                }

                for (int i = 0; i < properties.Count; i++)
                {

                }
            }
        }

        public readonly struct Entry
        {
            public readonly Object Target { get; }
            public readonly string Name { get; }

            public override bool Equals(object obj)
            {
                if (obj is Entry target)
                    return Equals(target);

                return false;
            }
            public bool Equals(Entry target)
            {
                if (this.Target != target.Target) return false;
                if (this.Name != target.Name) return false;

                return true;
            }

            public override int GetHashCode() => (Target, Name).GetHashCode();

            public readonly string TextCache { get; }
            public override string ToString() => TextCache;

            public Entry(Object target, string name)
            {
                this.Target = target;
                this.Name = name;

                TextCache = FormatEntryName(Target, Name);
            }

            public static bool operator ==(Entry right, Entry left) => right.Equals(left);
            public static bool operator !=(Entry right, Entry left) => right.Equals(left);
        }
#endif

        public static class TypeAnalyzer
        {
            public static Dictionary<Type, List<PropertyInfo>> Cache;

            public static List<PropertyInfo> Iterate(Type type)
            {
                if (Cache.TryGetValue(type, out var list))
                    return list;

                var properties = type.GetProperties(Flags);

                list = new List<PropertyInfo>(properties.Length);

                for (int i = 0; i < properties.Length; i++)
                {
                    if (ValidateProperty(properties[i]) == false) continue;

                    list.Add(properties[i]);
                }

                Cache.Add(type, list);

                return list;
            }

            public static bool ValidateProperty(PropertyInfo info)
            {
                if (info.GetMethod == null) return false;
                if (info.GetMethod.IsPublic == false) return false;

                if (info.SetMethod == null) return false;
                if (info.SetMethod.IsPublic == false) return false;

                return true;
            }

            static TypeAnalyzer()
            {
                Cache = new Dictionary<Type, List<PropertyInfo>>();
            }
        }
    }
}