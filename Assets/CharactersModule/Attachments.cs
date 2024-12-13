using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharactersModule
{
	// Можно было бы сделать его не Mono, но что от этого выиграю?
	// а так можно наслаждаться дебаггингом через Инспектор.
	[ExecuteAlways]
	public class Attachments : MonoBehaviour
	{
		private Animator animator;

		[SerializeField] private bool specificOffset;

		private List<Attachment> attachments = new(); 

		[Button, HideIf(nameof(specificOffset))]
		public Attachment Attach(GameObject prefab, HumanBodyBones bone, string subBoneKey = "")
		{
			return Attach( prefab, bone, subBoneKey, prefab.transform.localPosition );
		}

		[Button, ShowIf(nameof(specificOffset))]
		public Attachment Attach(
			GameObject prefab,
			HumanBodyBones bone,
			string subBoneKey,
			Vector3 offset)
		{
			var instance = Instantiate( prefab, GetBoneContainerInstance( bone ), false );
			InitAnimationSubBone( bone, subBoneKey );
			//attachment.transform.localPosition = prefab.transform.position;
			//attachment.transform.localRotation = prefab.transform.rotation;
			//maybe it's Editor only
			var attachment = instance.AddComponent<Attachment>();
			attachment.Bone = bone;
			attachment.subBoneKey = subBoneKey;
			attachment.Offset = offset;
			// if(bone == HumanBodyBones.Head) //temp 
			// 	attachment.transform.localScale = prefab.transform.localScale - (getAnimator().transform.lossyScale - Vector3.one);
			if (Application.isPlaying)
			{
				attachments.Add( attachment );
			}

			return attachment;
		}
		
		public void Remove(HumanBodyBones bone)
		{
			for (var index = attachments.Count - 1; index >= 0; index--)
			{
				Attachment attachment = attachments[ index ];
				if (attachment.Bone == bone)
				{
					attachments.RemoveAt( index );
					Destroy( attachment.gameObject );
				}
			}
		}

		public void Remove(Attachment attachment)
		{
			attachments.Remove( attachment );
			Destroy(attachment.gameObject);
		}
		
		[Button]
		public void SetAnimator(Animator animator)
		{
			this.animator = animator;
			foreach (Attachment attachment in attachments)
			{
				InitAnimationSubBone( attachment.Bone, attachment.subBoneKey );
			}
		}

		private void LateUpdate()
		{
			if (Application.isPlaying == false)
			{
				attachments = new List<Attachment>(GetComponentsInChildren<Attachment>());
			}
			foreach (Attachment attachment in attachments)
			{
				//copy bone position
				var boneTransform = getAnimator().GetBoneTransform( attachment.Bone );
				var containerInstance = GetBoneContainerInstance( attachment.Bone );
				containerInstance.position = boneTransform.position;
				containerInstance.rotation = boneTransform.rotation;
				//if (attachment.CopyScale)
				// {
				// 	containerInstance.localScale = new Vector3( 
				// 		containerInstance.localScale.x * (boneTransform.lossyScale.x / containerInstance.lossyScale.x),
				// 		containerInstance.localScale.y * (boneTransform.lossyScale.y / containerInstance.lossyScale.y),
				// 		containerInstance.localScale.z * (boneTransform.lossyScale.z / containerInstance.lossyScale.z) );
				// }

				if (string.IsNullOrEmpty( attachment.subBoneKey ))
				{
					attachment.transform.localPosition = attachment.Offset;
					continue;
				}
				
				//если есть специфический ключ привязки объекта, то нужно попробовать его найти под костью и если он
				//включен, то копировать его позицию. Если он выключен, то копировать позицию кости по умолчанию - Item
				//если же и она выключена - то приязываться к обычной позиции кости ( ничего не делать )
				var subBone = GetAnimationSubBone( boneTransform, attachment.subBoneKey );
				if (subBone == null)
				{
					attachment.transform.localPosition = attachment.Offset;
					continue;
				}

				attachment.transform.localPosition = subBone.transform.localPosition + attachment.Offset;
				attachment.transform.localRotation = subBone.transform.localRotation;
				//attachment.transform.localScale = subBone.transform.localScale;
			}
		}

		private void InitAnimationSubBone(HumanBodyBones bone, string subBoneKey)
		{
			if (string.IsNullOrEmpty( subBoneKey ))
				return;
			var boneTransform = getAnimator().GetBoneTransform( bone );
			foreach (Transform child in boneTransform)
			{
				if (child.name == subBoneKey)
					return;
			}

			var subBone = new GameObject( subBoneKey ).transform;
			if (Application.isEditor)
			{
				subBone.hideFlags = HideFlags.DontSave;
			}
			subBone.parent = boneTransform;
			subBone.localPosition = Vector3.zero;
			subBone.localRotation = Quaternion.identity;
			subBone.localScale = Vector3.one;
			subBone.gameObject.SetActive( false );
		}

		private Transform GetAnimationSubBone(Transform boneTransform, string subBoneKey)
		{
			Transform defaultBone = null;
			foreach (Transform child in boneTransform)
			{
				if (child.name == "Item")
				{
					defaultBone = child;
				}

				if (child.name == subBoneKey)
				{
					if (child.gameObject.activeInHierarchy)
						return child;
					else if (defaultBone != null)
						break;
				}
			}

			if (defaultBone != null && defaultBone.gameObject.activeInHierarchy)
				return defaultBone;
			return null;
		}

		private Transform GetBoneContainerInstance(HumanBodyBones humanBodyBones)
		{
			var boneName = "b_" + humanBodyBones;
			foreach (Transform child in transform)
			{
				if (child.name == boneName)
					return child;
			}
			
			var containerInstance = new GameObject( boneName ).transform;
			if (Application.isEditor)
			{
				containerInstance.hideFlags = HideFlags.DontSave;
			}
			containerInstance.parent = transform;
			containerInstance.localPosition = Vector3.zero;
			containerInstance.localRotation = Quaternion.identity;
			return containerInstance;
		}

		private Animator getAnimator()
		{
			if (Application.isPlaying)
			{
				if (animator == null)
				{
					animator = GetComponentInChildren<Animator>();
				}
				return animator;
			}
			return GetComponentInChildren<Animator>();
		}
	}
}