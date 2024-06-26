import { gql } from 'graphql-tag';

export const UPLOAD_IMAGE_MUTATION = gql`
  mutation UploadImage($b64Img: String!, $userId: String!) {
    uploadImage(b64Img: $b64Img, userId: $userId)
  }
`;

export const DATA_EXTRACTED_SUBSCRIPTION = gql`
  subscription {
    dataExtracted
  }
`;