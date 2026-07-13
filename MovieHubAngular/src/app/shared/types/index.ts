export type ActiveView = 'home' | 'genero' | 'detalle' | 'favoritos';

export interface AuthResponse {
  token: string;
  expiration: string;
  userId: number;
  userName: string;
  email: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  userName: string;
  email: string;
  password: string;
}
