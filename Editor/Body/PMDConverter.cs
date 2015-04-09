﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace MMD.Body.Converter
{
    public class PMDConverter
    {
        MMD.Format.PMDFormat format;
        MMD.Body.Argument.PMDArgument argument;

        string directory;
        string filename;

        GameObject boneRoot;

        public PMDConverter(MMD.Body.Argument.PMDArgument argument)
        {
            if (!argument.path.ToLower().Contains(".pmd"))
                throw new System.ArgumentException("PMDファイル以外のファイルがロードされました");

            format = new MMD.Format.PMDFormat();
            format.Read(argument.path, argument.scale);

            directory = System.IO.Path.GetDirectoryName(argument.path);
            filename = System.IO.Path.GetFileNameWithoutExtension(argument.path);

            this.argument = argument;
        }

        GameObject CreateRoot()
        {
            var root = new GameObject(format.Header.modelName);
            boneRoot = new GameObject("Bones");

            boneRoot.transform.parent = root.transform;

            return root;
        }

        void EntryMesh(Mesh mesh)
        {
            AssetDatabase.CreateAsset(mesh, directory + "/" + filename + ".asset");
        }

        void EntryMaterials(Material[] materials)
        {
            AssetDatabase.CreateFolder(directory, "Materials");
            for (int i = 0; i < materials.Length; ++i)
            {
                AssetDatabase.CreateAsset(materials[i], directory + "/Materials/材質" + (i + 1).ToString() + ".mat");
            }
        }

        void EntryPhysicMaterials(List<MMD.Builder.PMD.Physics> materials)
        {
            AssetDatabase.CreateFolder(directory, "Physics");
            for (int i = 0; i < materials.Count; ++i)
            {
                AssetDatabase.CreateAsset(materials[i].material, directory + "/Physics/" + materials[i].name + ".physicMaterial");
            }
        }

        void ConnectBones(GameObject[] bones, GameObject root)
        {
            bones[0].transform.parent = root.transform;
        }

        void EntryPrefab(GameObject root)
        {
            AssetDatabase.CreateAsset(root, directory + "/" + filename + ".prefab");
        }

        public void Import()
        {
            var root = CreateRoot();

            var renderer = root.AddComponent<SkinnedMeshRenderer>();
            var builder = new MMD.Builder.PMD.ModelBuilder(renderer);
            builder.Read(format, argument.shader);

            // ここより下でアセットの登録を行う
            EntryMesh(builder.Mesh);
            EntryMaterials(builder.Materials);
            EntryPhysicMaterials(builder.Physics);
            ConnectBones(builder.Bones, boneRoot);
            
            // 剛体のほうをいい加減取り組む



            EntryPrefab(root);

            /// TODO
            /// 剛体のAdapter/Builder部分を書く
            /// シェーダを書く
        }
    }
}
