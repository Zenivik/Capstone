using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTexturePool : MonoBehaviour
{
    public static RenderTexturePool Instance;
    public int maxSize = 100;
    private List<PoolItem> Pool = new List<PoolItem>();

    public class PoolItem
    {
        public RenderTexture Texture;
        public bool Used;
    }

    private void Awake()
    {
        Instance = this;
    }

    // Gets a new temporary texture from the pool
    public PoolItem GetTexture()
    {
        // Check all Pool Items. Are any unused?
        // If so, take the first unused one we come across mark it as used and return it
        foreach(var poolItem in Pool)
        {
            if (!poolItem.Used)
            {
                poolItem.Used = true;
                return poolItem;
            }
        }

        // Are none of them unused? Expand
        if(Pool.Count >= maxSize)
        {
            //Debug.LogError("Pool is full!");
            throw new OverflowException();
        }

        var newPoolItem = CreateTexture();
        Pool.Add(newPoolItem);

        //Debug.Log($"New RenderTexture created, pool is now {Pool.Count} items big");

        newPoolItem.Used = true;
        return newPoolItem;
    }

    public void ReleaseTexture(PoolItem item)
    {
        // When releasing a texture, simply mark it as used.
        // No need to overwrite it or anything
        item.Used = false;
    }

    public void ReleaseAllTextures()
    {
        foreach(var poolItem in Pool)
        {
            ReleaseTexture(poolItem);
        }
    }

    private PoolItem CreateTexture()
    {
        var newTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.DefaultHDR);
        newTexture.Create();

        return new PoolItem { Texture = newTexture, Used = false };
    }

    private void DestroyTexture(PoolItem item)
    {
        item.Texture.Release();
        Destroy(item.Texture);
    }

    private void OnDestroy()
    {
        foreach(var poolItem in Pool)
        {
            DestroyTexture(poolItem);
        }
    }
}



