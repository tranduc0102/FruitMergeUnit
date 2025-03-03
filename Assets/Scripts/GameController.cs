using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class GameController : Singleton<GameController>
{
   [Space]
   [Header("Setup")]
   [SerializeField] private ModelFruit model;

   [SerializeField] private UINextFruit _uiNextFruit;

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

   [Space] [Header("Game State")] 
   [SerializeField] private bool isLose;

   private const float POSITIONNOMOVE = 4.7f;
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
      
      if (Input.GetMouseButtonDown(0))
      {
         OnDown();
      }
      if (Input.GetMouseButton(0))
      {
         OnMove();
      }
      if (Input.GetMouseButtonUp(0))
      {
         OnUp();
      }
   }
   private void OnDown()
   {
      if (!isDelay)
      {
         canSwipe = true;
         MoveObject(Input.mousePosition);
      }
   }

   private void OnDown(Touch touch)
   {
      if (!isDelay)
      {
         canSwipe = true;
         MoveObject(touch.position);
      }
   }
   private void OnMove()
   {
      if (canSwipe)
      {
         MoveObject(Input.mousePosition);
      }
   }

   private void OnMove(Touch touch)
   {
      if (canSwipe)
      {
        MoveObject(touch.position);
      }
   }

   private void OnUp()
   {
      if(!canSwipe) return;
      _fruit.OnFall();
      isDelay = true;
      canSwipe = false;
      if(_fruit) _fruit.transform.SetParent(objPool);
      _fruit = null;
      DOVirtual.DelayedCall(timeSpawn, delegate
      {
         SpawnFruit();
         isDelay = false;
      });
   }
   private void MoveObject(Vector3 position)
   {
      Vector3 pos = Camera.main.ScreenToWorldPoint(position);
      if(pos.x > POSITIONNOMOVE || pos.x < -POSITIONNOMOVE) return;
      objectSpawn.transform.position = new Vector3(pos.x, objectSpawn.transform.position.y, 0f);
   }
   private void NextFruit()
   {
      indexNextFruit = Random.Range(0, model.LimitFruit);
      _uiNextFruit.SetNextFruit(model.DataFruit[indexNextFruit].GetComponentInChildren<SpriteRenderer>().sprite);
   }
   private void SpawnFruit()
   {
      if(isLose) return;
      _fruit = PoolingManager.Spawn(model.DataFruit[indexNextFruit], objectSpawn.position, quaternion.identity, objectSpawn);
      _fruit.Init(indexNextFruit, MergeFruit, GameOver);
      _fruit.transform.localScale = Vector3.zero;
      _fruit.transform.DOScale(Vector3.one, 0.2f);
      NextFruit();
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
      isLose = true;
   }
}
