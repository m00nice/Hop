using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using XNode;

namespace SceneFlow
{
    public class Choise : MonoBehaviour
    {

        public TMP_Text choiseText;
        public Button button;
        public ChoiseNode choiseNode;

        // Start is called before the first frame update
        public Choise Create(ChoiseNode choiseNode, RectTransform parent)
        {
            Choise newChoise = Instantiate(this.gameObject, parent).GetComponent<Choise>();
            newChoise.choiseNode = choiseNode;
            if (choiseNode == null)
            {
                newChoise.choiseText.text = "Next";
            }
            else
            {
                newChoise.choiseText.text = choiseNode.choiseText;
            }

            newChoise.button.onClick.AddListener(newChoise.SelectChoise);
            return newChoise;
        }


        public void SelectChoise()
        {

            Next(choiseNode);
        }

        void Next(ChoiseNode choise)
        {

            if (choise == null)
            {
                GameTextFlowSystem.instance.Next(null);
            }
            else
            {
                IEnumerable<NodePort> ports = choise.Ports;
                using (var sequenceEnum = ports.GetEnumerator())
                {
                    while (sequenceEnum.MoveNext())
                    {
                        List<NodePort> nodePorts = sequenceEnum.Current.GetConnections();
                        if (nodePorts != null && nodePorts.Count > 0)
                        {
                            for (int i = 0; i < nodePorts.Count; i++)
                            {
                                if (nodePorts[i].node.GetType() == typeof(TextNode))
                                {
                                    TextNode textNode = nodePorts[i].node as TextNode;
                                    GameTextFlowSystem.instance.Next(textNode);
                                }
                                else if (nodePorts[i].node.GetType() == typeof(NewSystemNode))
                                {
                                    NewSystemNode newSystem = (nodePorts[i].node as NewSystemNode);
                                    TextNode nextNode = null;
                                    if (newSystem.startNode != "")
                                    {
                                        Node node = newSystem.textNodeSystem.nodes.Find(c => c.name.ToLower() == newSystem.startNode.ToLower());
                                        if (node != null && node.GetType() == typeof(TextNode))
                                        {
                                            nextNode = node as TextNode;
                                        }
                                    }
                                    else
                                    {
                                        Node node = newSystem.textNodeSystem.nodes[0];
                                        if (node != null && node.GetType() == typeof(TextNode))
                                        {
                                            nextNode = node as TextNode;
                                        }
                                    }

                                    TextNode textNode = nextNode;
                                    GameTextFlowSystem.instance.Next(textNode);
                                }
                                else
                                {
                                    Debug.Log("Choise didn't leave to new TextNode");
                                }

                            }
                        }
                    }
                }
            }

        }
    }
}

