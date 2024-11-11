using System;
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

		[Button, HideIf(nameof(specificOffset))]
		public void Attach(GameObject prefab, HumanBodyBones bone, string subBoneKey = "")
		{
			Attach( prefab, bone, subBoneKey, Vector3.zero );
		}

		[Button, ShowIf(nameof(specificOffset))]
		public void Attach(
			GameObject prefab,
			HumanBodyBones bone,
			string subBoneKey,
			Vector3 offset)
		{
			var attachment = Instantiate( prefab, GetBoneContainerInstance( bone ), false );
			InitAnimationSubBone( bone, subBoneKey );
			//attachment.transform.localPosition = prefab.transform.position;
			//attachment.transform.localRotation = prefab.transform.rotation;
			//maybe it's Editor only
			var attachmentPreview = attachment.AddComponent<AttachmentPreview>();
			attachmentPreview.Bone = bone;
			attachmentPreview.subBoneKey = subBoneKey;
			attachmentPreview.Offset = offset;
		}

		private void LateUpdate()
		{
			var attachments = GetComponentsInChildren<AttachmentPreview>();
			foreach (AttachmentPreview attachment in attachments)
			{
				//copy bone position
				var boneTransform = getAnimator().GetBoneTransform( attachment.Bone );
				var containerInstance = GetBoneContainerInstance( attachment.Bone );
				containerInstance.position = boneTransform.position;
				containerInstance.rotation = boneTransform.rotation;
				containerInstance.localScale = boneTransform.localScale;

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
				attachment.transform.localScale = subBone.transform.localScale;
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
			containerInstance.parent = getAnimator().transform;
			containerInstance.localPosition = Vector3.zero;
			containerInstance.localRotation = Quaternion.identity;
			return containerInstance;
		}

		private Animator getAnimator() => GetComponent<Animator>();
	}
}