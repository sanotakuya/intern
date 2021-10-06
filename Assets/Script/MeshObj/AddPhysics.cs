﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

//-----------------------------------------------------------------------------
//! [制作者]		小野龍大
//!	[最終更新日]	2021/10/06
//! [内容]		Meshのあるオブジェクトに物理と当たり判定、同期を追加する
//-----------------------------------------------------------------------------
public class AddPhysics : MonobitEngine.MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //!	private変数
    //-----------------------------------------------------------------------------
    [SerializeField] private float mass = 1;
    [SerializeField] private float drag = 0;
    [SerializeField] private float angularDrag = 0.05f;
    [SerializeField] private CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;


    void Start()
    {
        GameObject thisObj = this.gameObject;
        MeshCollider meshCollider = null;
        Rigidbody rigidbody = null;
        MonobitView monobitView = null;

        // MeshColliderの設定
        meshCollider = thisObj.gameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;

        // rigidbodyの設定
        rigidbody = thisObj.gameObject.AddComponent<Rigidbody>();
        rigidbody.mass = mass;
        rigidbody.drag = drag;
        rigidbody.angularDrag = angularDrag;
        rigidbody.collisionDetectionMode = collisionDetectionMode;

        // 同期設定
        monobitView = thisObj.gameObject.AddComponent<MonobitView>();
        monobitView.ObservedComponents.Add(thisObj.gameObject.AddComponent<MonobitTransformView>());
    }
}
