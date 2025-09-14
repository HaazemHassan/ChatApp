/**
 * List of API endpoints that do not require authentication
 * These endpoints will not have the Authorization header attached
 */
export const NON_AUTHENTICATED_ENDPOINTS = [
  '/authentication/login',
  '/authentication/register',
  '/authentication/forgot-password',
  '/authentication/reset-password',
  '/authentication/verify-email',
  '/authentication/resend-verification',
  '/public/',
  '/health',
  '/version',
];

/**
 * Check if a URL should skip authentication
 * @param url The request URL to check
 * @returns true if the URL should skip authentication
 */
export function shouldSkipAuthentication(url: string): boolean {
  return NON_AUTHENTICATED_ENDPOINTS.some((endpoint) => url.includes(endpoint));
}
