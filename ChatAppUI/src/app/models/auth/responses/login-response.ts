import { User } from '../../interfaces/userInterface';

export interface LoginResponse {
  accessToken: string;
  refreshToken?: RefreshTokenDTO;
  user: User;
}

export interface RefreshTokenDTO {
  token: string;
  userId: number;
  expirationDate: Date;
  createdAt: Date;
}
