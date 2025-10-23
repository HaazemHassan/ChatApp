export function getCookieValue(cookieName: string): string | null {
  const cookies = document.cookie.split(';');
  for (let cookie of cookies) {
    const trimmedCookie = cookie.trim();
    const separatorIndex = trimmedCookie.indexOf('=');

    if (separatorIndex === -1) {
      continue;
    }
    const name = trimmedCookie.substring(0, separatorIndex);

    if (name === cookieName) {
      const value = trimmedCookie.substring(separatorIndex + 1);
      return decodeURIComponent(value);
    }
  }
  return null;
}
