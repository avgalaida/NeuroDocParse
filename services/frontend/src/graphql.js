import { gql } from 'graphql-tag';

export const UPLOAD_IMAGE_MUTATION = gql`
  mutation UploadImage($b64Img: String!, $userId: String!, $requestId: String!, $requestType: String!, $model: String!) {
    uploadImage(b64Img: $b64Img, userId: $userId, requestId: $requestId, requestType: $requestType, model: $model)
  }
`;

export const UPLOAD_IMAGE_WITH_FIELDS_MUTATION = gql`
  mutation UploadImageWithFields($b64Img: String!, $userId: String!, $requestId: String!, $model: String!, $fields: JSON!) {
    uploadImageWithFields(b64Img: $b64Img, userId: $userId, requestId: $requestId, model: $model, fields: $fields)
  }
`;

export const DATA_EXTRACTED_SUBSCRIPTION = gql`
  subscription DataExtracted($requestId: String!) {
    dataExtracted(requestId: $requestId)
  }
`;