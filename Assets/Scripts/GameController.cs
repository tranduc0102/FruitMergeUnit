using System.Collections.Generic;
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

   [SerializeField] private GameObject _scrollLine;

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

   [Space] [Header("Tips")] [SerializeField]
   private List<InfoFruit> allFruits;

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
               if (isUseTip1 || isUseTip2 || isUseTip3 || isUseTip4)
               {
                  if (isUseTip2)
                  {
                     ChoiceRemoveFruit(touch.position);
                  }
                  return;
               }
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

      if (Input.GetKeyDown(KeyCode.A))
      {
         UseTipRemoveOneFruit();
      }
      if (Input.GetMouseButtonDown(0))
      {
         // Khi chạm tay xuống
         if (isUseTip1 || isUseTip2 || isUseTip3 || isUseTip4)
         {
            if (isUseTip2)
            {
               ChoiceRemoveFruit(Input.mousePosition);
            }
            return;
         }
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
         canSwipe = true; // Biến kiểm tra xem có được vuốt di không
         MoveObject(Input.mousePosition); // sau đó thì di chuyển GameObject theo vị trí bấm xuống
         _scrollLine.SetActive(true);
      }
   }

   private void OnDown(Touch touch)
   {
      if (!isDelay)
      {
         canSwipe = true;
         MoveObject(touch.position);
         _scrollLine.SetActive(true);
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
      _scrollLine.SetActive(false);
      if(!canSwipe) return; //Nếu không được vuốt thì return
      _fruit.OnFall(); // thực hàm thả fruit rơi
      isDelay = true; // bắt đầu thời gian delay spawn next fruit
      canSwipe = false; // khi bỏ tay ra thì canSwipe bằng false tức ko được vuốt nữa
      if (_fruit)
      {
         allFruits.Add(_fruit); //Thêm fruit hiện tại vào danh sách
         _fruit.transform.SetParent(objPool); //Set lại vị trí của fruit vừa thả tay ra
         _fruit = null;// thả rồi thì fruit đang ở trên tay = null tức là không còn nữa
      }
      DOVirtual.DelayedCall(timeSpawn, delegate  // Thực hiện delay spawn next fruit
      {
         SpawnFruit();
         isDelay = false;
      });
   }
   private void MoveObject(Vector3 position)
   {
      // vector3 vị trí đang chạm với camera
      Vector3 pos = Camera.main.ScreenToWorldPoint(position);
      if(pos.x > POSITIONNOMOVE || pos.x < -POSITIONNOMOVE) return; //Kiểm tra vị tr đó có nằm ngoài vị trí cho phép di chuyển
      objectSpawn.transform.position = new Vector3(pos.x, objectSpawn.transform.position.y, 0f);
      // nếu không nằm ngoài thì di chuyển gameobject theo vị trí đang bấm
   }
   private void NextFruit()
   {
      indexNextFruit = Random.Range(0, model.LimitFruit);
      _uiNextFruit.SetNextFruit(model.DataFruit[indexNextFruit].GetComponentInChildren<SpriteRenderer>().sprite);
   }
   private void SpawnFruit()
   {
      if(isLose) return;
      _fruit = PoolingManager.Spawn(model.DataFruit[indexNextFruit], objectSpawn.position, quaternion.identity, objectSpawn); // sẽ random next fruit và spawn tại vị trí objectSpawn rồi set thằng cha của nó là objectSpawn
      _fruit.Init(indexNextFruit, MergeFruit, GameOver); //Khởi tạo fruit mới
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
      
      if (allFruits.Contains(fruit1)) allFruits.Remove(fruit1);
      if (allFruits.Contains(fruit2)) allFruits.Remove(fruit2);
      
      DOVirtual.DelayedCall(1.2f, delegate
      {
         PoolingManager.Despawn(effect.gameObject);
      });
   }

   private void GameOver()
   {
      isLose = true;
   }

   #region Tips

   private bool isUseTip1 = false;
   private bool isUseTip2 = false;
   private bool isUseTip3 = false;
   private bool isUseTip4 = false;
   private void RemoveAllFruitLevel1And2()
   {
      foreach (var fruit in allFruits)
      {
         if (fruit.Level == 0 || fruit.Level == 1)
         {
            PoolingManager.Spawn(effeectMerge, fruit.transform.position, quaternion.identity, objPool);
            PoolingManager.Despawn(fruit.gameObject);
            allFruits.Remove(fruit);
         }
      }

      isUseTip1 = true;
   }

   public void UseTipRemoveOneFruit()
   {
      foreach (var fruit in allFruits)
      {
         //thực hiện anim hiện các quả có thể chọn
      }

      isUseTip2 = true;
   }
   private void ChoiceRemoveFruit(Vector3 position)
   {
      Vector3 inputPos = Camera.main.ScreenToWorldPoint(position);
      RaycastHit2D hit = Physics2D.Raycast(inputPos, Vector2.down);
      if (hit.collider.TryGetComponent(out InfoFruit fruit1))
      {
         PoolingManager.Spawn(effeectMerge, fruit1.transform.position, quaternion.identity, objPool);
         PoolingManager.Despawn(fruit1.gameObject);
         allFruits.Remove(fruit1);
      }
   }
   

   #endregion
}
