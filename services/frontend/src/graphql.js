import { gql } from 'graphql-tag';

export const UPLOAD_IMAGE_MUTATION = gql`
  mutation UploadImage($b64Img: String!, $userId: String!, $requestId: String!, $requestType: String!, $model: String!) {
    uploadImage(b64Img: $b64Img, userId: $userId, requestId: $requestId, requestType: $requestType, model: $model)
  }
`;

export const DATA_EXTRACTED_SUBSCRIPTION = gql`
  subscription DataExtracted($requestId: String!) {
    dataExtracted(requestId: $requestId)
  }
`;