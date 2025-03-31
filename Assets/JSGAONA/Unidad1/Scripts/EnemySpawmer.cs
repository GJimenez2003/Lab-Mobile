using UnityEngine;

namespace Assets.JSGAONA.Unidad1.Scripts {

    // Este script se emplea para gestionar la logica de aparicion de los enemigos
    public class EnemySpawmer : MonoBehaviour {
        
        // Variables visibles desde el inspector de Unity
        [SerializeField] private Transform player;
        [SerializeField] private GameObject prefabEnemy;
        [SerializeField] private Transform[] points;
        [SerializeField] private float[] distance = { 0, 20f, 40f, float.MaxValue };

        // Variables ocultas desde el inspector de Unity
        private Enemy[] enemies;
        private CullingGroup cullingGroup;
        private BoundingSphere[] boundingSpheres;


        // Metodo de llamada de Unity, se llama una unica vez al iniciar el app, se ejecuta despues
        // de Awake, se realiza la asignacion de variables y configuracion del script
        private void Start() {
            enemies = new Enemy[points.Length];
            boundingSpheres = new BoundingSphere[points.Length];

            // Se recorre todos la lista para inicializarlos con nuevos objetos BoundingSphere
            for(int i = 0; i < boundingSpheres.Length; i++){
                boundingSpheres[i] = new BoundingSphere(points[i].position, 1.0f);
            }

            // Se gestiona de manera eficiente la activación y desactivación de objetos en función
            // de su visibilidad desde la camara principal
            cullingGroup = new CullingGroup();
            cullingGroup.SetBoundingSpheres(boundingSpheres);
            cullingGroup.SetBoundingSphereCount(boundingSpheres.Length);
            cullingGroup.SetBoundingDistances(distance);
            cullingGroup.SetDistanceReferencePoint(player.position);
            cullingGroup.targetCamera = Camera.main;
            cullingGroup.onStateChanged += SpawnPointCullingGroupChanged;
        }


        // Metodo de llamada de Unity, se llama en cada frame del computador
        // Se realiza la logica de control entre el personaje y el cullingGroup
        private void Update() {
            cullingGroup.SetDistanceReferencePoint(player.position);
        }


        // Metodo de llamada de Unity, se llama una unica vez cuando el GameObejct es destruido
        private void OnDestroy() {
            if(cullingGroup != null) {
                cullingGroup.onStateChanged -= SpawnPointCullingGroupChanged;
                cullingGroup.Dispose();
                cullingGroup = null;
            }
            if(enemies != null) enemies = null;
            if(boundingSpheres != null) boundingSpheres = null;
        }


        // Se emplea este metodo para saber la informacion del estado del objeto monitoreado
        private void SpawnPointCullingGroupChanged(CullingGroupEvent sphere) {
            bool enemyExist = enemies[sphere.index] != null;

            // Genera un enemigo si no existe y el personaje esta en rango cercano
            if(!enemyExist && sphere.currentDistance <= 2){
                enemies[sphere.index] = SpawnEnemy(points[sphere.index].position);
            }

            // Activa el enemigo si el personaje esta en rango moderado
            if(enemyExist && sphere.currentDistance == 2){
                enemies[sphere.index].gameObject.SetActive(true);
            }

            // Desactiva el enemigo si el personaje se aleja demaciado
            if(enemyExist && sphere.currentDistance == 3){
                enemies[sphere.index].gameObject.SetActive(false);
            }
        }
        

        // Metodo que se emplea para poder gestionar enemigos
        private Enemy SpawnEnemy(Vector3 position){
            GameObject newEnemy = Instantiate(prefabEnemy, position, Quaternion.identity, transform);
            if(newEnemy.TryGetComponent(out Enemy enemy)){
                enemy.Player = player;
                return enemy;
            }
            return null;
        }
    }
}