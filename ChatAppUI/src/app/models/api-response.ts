export interface ApiResponse<T> {
  statusCode: number;
  succeeded: boolean;
  message: string;
  errors?: string[] | null;
  data?: T | null;
  meta?: any;
}
