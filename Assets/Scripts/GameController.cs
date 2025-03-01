using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : Singleton<GameController>
{
   [Space]
   [Header("Setup")]
   [SerializeField] private ModelFruit model;

   public ModelFruit Model
   {
      get => model;
   }
   [SerializeField] private Transform objectSpawn;
   [SerializeField] private Transform objPool;
   [SerializeField] private InfoFruit _fruit;
   [SerializeField] private ParticleSystem effeectMerge;

   private int indexNextFruit;

   [Space]
   [Header("Time Spawn")]
   [SerializeField] private float timeSpawn;
   [SerializeField] private float timer;


   private const float POSITIONNOMOVE = 5f;
   private bool canSwipe;
   private bool isDelay;

   private void Start()
   {
      NextFruit();
      SpawnFruit();
   }

   private void Update()
   {
      if (Input.touchCount > 0)
      {
         Touch touch = Input.GetTouch(0);
         switch (touch.phase)
         {
            case TouchPhase.Began:
               OnDown(touch);
               break;
            case TouchPhase.Moved:
               OnMove(touch);
               break;
            case TouchPhase.Ended:
               OnUp();
               break;
         }
      }
   }

   private void OnDown(Touch touch)
   {
      if (!isDelay)
      {
         canSwipe = true;
         MoveObject(touch);
      }
   }

   private void OnMove(Touch touch)
   {
      if (canSwipe)
      {
        MoveObject(touch);
      }
   }

   private void OnUp()
   {
      if(!canSwipe) return;
      _fruit.OnFall();
      NextFruit();
      isDelay = true;
      canSwipe = false;
      DOVirtual.DelayedCall(timeSpawn, delegate
      {
         SpawnFruit();
         isDelay = false;
      });
   }

   private void MoveObject(Touch touch)
   {
      Vector3 pos = Camera.main.ScreenToWorldPoint(touch.position);
      if(pos.x > POSITIONNOMOVE || pos.x < -POSITIONNOMOVE) return;
      objectSpawn.transform.position = new Vector3(pos.x, objectSpawn.transform.position.y, 0f);
   }
   private void NextFruit()
   {
      if(_fruit) _fruit.transform.SetParent(objPool);
      _fruit = null;
      indexNextFruit = Random.Range(0, model.LimitFruit);
      // UI next fruit
   }
   private void SpawnFruit()
   {
      _fruit = PoolingManager.Spawn(model.DataFruit[indexNextFruit], objectSpawn.position, quaternion.identity, objectSpawn);
      _fruit.Init(indexNextFruit, MergeFruit, GameOver);
   }

   private void MergeFruit(InfoFruit fruit1, InfoFruit fruit2, int level)
   {
      if(fruit1.IsColider && fruit2.IsColider) return;
      
      Vector3 newPosSpawn = (fruit1.transform.position + fruit2.transform.position) / 2;
      
      ParticleSystem effect = PoolingManager.Spawn(effeectMerge, newPosSpawn, Quaternion.identity, objPool);
      effect.Play();
      InfoFruit newFruit = PoolingManager.Spawn(model.DataFruit[level], newPosSpawn, quaternion.identity, objPool);
      newFruit.Init(level, MergeFruit, GameOver, true);
      PoolingManager.Despawn(fruit1.gameObject);
      PoolingManager.Despawn(fruit2.gameObject);
      DOVirtual.DelayedCall(1.2f, delegate
      {
         PoolingManager.Despawn(effect.gameObject);
      });
   }

   private void GameOver()
   {
      
   }
}
