using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EfectosCerebro : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    public float velocidadGiro = 180f; // Velocidad de giro en grados por segundo
    
    [Header("Configuración de Pulso")]
    public float velocidadPulso = 1f; // Velocidad del pulso de brillo
    public float amplitudPulso = 0.2f; // Amplitud del pulso de brillo
    
    [Header("Configuración de Tamaño")]
    public float escalaMinima = 0.8f; // Escala mínima del efecto de brillo
    public float escalaMaxima = 1.2f; // Escala máxima del efecto de brillo
    public bool usarEscalaPersonalizada = true; // Si debe usar los valores personalizados de escala

    private Vector3 escalaInicial;
    private Transform brilloTransform; // Referencia al objeto de brillo

    void Start()
    {
        // Buscar el objeto de brillo como hijo del cerebro
        brilloTransform = transform.Find("EfectoBrillo");

        // Almacenar la escala inicial del objeto de brillo
        if (brilloTransform != null)
        {
            escalaInicial = brilloTransform.localScale;
        }
    }    void Update()
    {
        // Girar el cerebro sobre su propio eje Y
        transform.Rotate(0, 0, velocidadGiro * Time.deltaTime);

        // Pulsar el efecto de brillo
        if (brilloTransform != null)
        {
            if (usarEscalaPersonalizada)
            {
                // Usar valores personalizados de escala mínima y máxima
                float factorPulso = (Mathf.Sin(Time.time * velocidadPulso) + 1f) * 0.5f; // Convertir de -1,1 a 0,1
                float escalaActual = Mathf.Lerp(escalaMinima, escalaMaxima, factorPulso);
                
                // Aplicar escala manteniendo las proporciones originales
                float proporcion = escalaActual / 1.0f; // Proporción respecto a escala 1
                brilloTransform.localScale = Vector3.Scale(escalaInicial, new Vector3(proporcion, proporcion, proporcion));
                
                // Debug para verificar los valores
                // Debug.Log("Escala actual: " + escalaActual + " - Factor: " + factorPulso);
            }
            else
            {
                // Usar el método original con amplitud
                float pulso = Mathf.Sin(Time.time * velocidadPulso) * amplitudPulso;
                brilloTransform.localScale = escalaInicial + Vector3.one * pulso;
            }
        }
    }
}
//a