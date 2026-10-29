using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace UnityEditor.Rendering.Universal
{
    [VolumeComponentEditor(typeof(SceneLight))]
    sealed class SceneLightEditor : VolumeComponentEditor
    {
        SerializedDataParameter m_AmbientSky;
        SerializedDataParameter m_AmbientGround;
        SerializedDataParameter m_AmbientEquator;
        
     
        SerializedDataParameter m_AmbientCubemap;
        SerializedDataParameter m_AmbientCubemapExposure;

        SerializedDataParameter m_Fog;
        SerializedDataParameter m_FogColor;
        SerializedDataParameter m_StartDistance;
        SerializedDataParameter m_EndDistance;

       


        public override void OnEnable()
        {
            var o = new PropertyFetcher<SceneLight>(serializedObject);

            m_AmbientSky = Unpack(o.Find(x => x.ambientSky));
            m_AmbientEquator = Unpack(o.Find(x => x.ambientEquator));
            m_AmbientGround = Unpack(o.Find(x => x.ambientGround));

            m_AmbientCubemap = Unpack(o.Find(x => x.ambientCubemap));
            m_AmbientCubemapExposure = Unpack(o.Find(x => x.ambientCubemapExposure));

            m_Fog = Unpack(o.Find(x => x.fog));
            m_FogColor = Unpack(o.Find(x => x.fogColor));
            m_StartDistance = Unpack(o.Find(x => x.startDistance));
            m_EndDistance = Unpack(o.Find(x => x.endDistance));

          
        }


        public override void OnInspectorGUI()
        {

            EditorGUILayout.LabelField("AmbientLight", EditorStyles.miniLabel);
            PropertyField(m_AmbientSky);
            PropertyField(m_AmbientEquator);
            PropertyField(m_AmbientGround);


            EditorGUILayout.Space();

            PropertyField(m_AmbientCubemap);
            PropertyField(m_AmbientCubemapExposure);

            EditorGUILayout.LabelField("Fog", EditorStyles.miniLabel);
            PropertyField(m_Fog);
            m_Fog.value.boolValue = m_Fog.overrideState.boolValue;
            PropertyField(m_FogColor);
            PropertyField(m_StartDistance);
            PropertyField(m_EndDistance);

        }
    }
}
