using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{

    public GameObject[] objects; //lista de objetos
    public GameObject pendingObject; //objeto selecionado
    
    [SerializeField] private Material[] materialPlacement; //materiais pra indicar por cor se pode ou não colocar um novo objeto ali

    private Vector3 pos; //posição do obj
    private RaycastHit hit;

    [SerializeField] private LayerMask layerMask;

    public float rotateAmount;

    public float gridSize;
    bool gridOn = true;
    public bool canPlace = true;
    [SerializeField] private Toggle gridToggle;


    void Update()
    {
        if(pendingObject != null) //checa se existe um objeto selecionado
        {
            UpdateMaterials(); //atualiza a cor pra definir se pode ou nao colocar lá

            if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer) //se for uma plataforma com touchscreen
            {
                if (!EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId)) //checa se o toque esta batendo em um botao
                {
                    MoveObjectOnMap(); //se NAO estiver tocando num botao atualiza a posicao do objeto no mapa
                }
            
            }else if (!EventSystem.current.IsPointerOverGameObject()) //else se estiver no pc usa o ponteiro do mouse
            {
                MoveObjectOnMap(); //se o ponteiro do mouse NAO estiver sobre um botao atualiza a posicao do objeto no mapa
            }

            //DESLIGADA pq a função passou de clicar com o mouse pra apertar um botão na tela
            //if (Input.GetMouseButtonDown(0) && canPlace) //se clicar com o botão esquerdo & canPlace for true
            //{
            //    PlaceObject();
            //}

            if (Input.GetKeyDown(KeyCode.R)) //se apertar a tecla R ele gira o objeto
            {
                RotateObject();
            }
        }
    }

    void MoveObjectOnMap()
    {
        if (gridOn) //se a grid estiver ligada
        {
            //pega a posição de cada coord do mouse e arredonda elas
            pendingObject.transform.position = new Vector3(
                RoundToNearestGrid(pos.x),
                RoundToNearestGrid(pos.y),
                RoundToNearestGrid(pos.z)
                );
        }
        else //se a grid estiver desligada move o objeto livremente 
        {
            pendingObject.transform.position = pos; //movimenta o objeto
        }
    }

    public void PlaceObject()
    {
        pendingObject.GetComponent<MeshRenderer>().material = materialPlacement[2]; //define a cor final ao posicionar o objeto
        pendingObject = null; //o objeto que estava selecionado não tá selecionado mais
    }

    public void RotateObject()
    {
        pendingObject.transform.Rotate(Vector3.up, rotateAmount); //up -> gira no y, rotateAmount -> variavel definida lá em cima
    }

    private void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //posição que vai colocar o objeto sendo segurado

        if(Physics.Raycast(ray, out hit, 1000, layerMask)) //esse 1000 é a distância que ele vai, pode trocar por uma variavel se quiser //layermask vai ser pra impedir que construa coisa sobre coisa
        {
            pos = hit.point; //o point pega o impact point no worldspace, basicamente diz pro jogo onde colocar o objeto
        }
    }
    
    void UpdateMaterials()
    {
        if (canPlace)
        {
            //se canPlace for true, coloca o material 0 do array
            pendingObject.GetComponent<MeshRenderer>().material = materialPlacement[0];
        }
        if (!canPlace)
        {
            //se for false coloca o material 1
            pendingObject.GetComponent<MeshRenderer>().material = materialPlacement[1];
        }
    }

    public void SelectObject(int index) //seleciona o objeto pelo index dele no array objects
    {
        pendingObject = Instantiate(objects[index], pos, transform.rotation);
        pendingObject.name = objects[index].name;
        materialPlacement[2] = pendingObject.GetComponent<MeshRenderer>().material; //coloca o material original do objeto como o usado pós posicionar
    }

    public void ToggleGrid() //liga desliga a grid
    {
        if (gridToggle.isOn)
        {
            gridOn = true;
        }
        else
        {
            gridOn = false;
        }
    }

    float RoundToNearestGrid(float pos)
    {
        float xDiff = pos % gridSize; //calcula o resto da posição pelo grid size

        //aí subtrai ou soma a posição pela diferença pra colocar a posição no grid mais próximo
        pos -= xDiff; 

        if(xDiff > (gridSize / 2))
        {
            pos += gridSize;
        }
        return pos;
    }
}
