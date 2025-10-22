import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class GoogleSignInLoaderService {
  private loaded = false;

  async loadScript(): Promise<void> {
    if (this.loaded) return;

    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.onload = () => {
        this.loaded = true;
        resolve();
      };
      script.onerror = () => reject(new Error('Failed to load Google Sign-In script'));
      document.body.appendChild(script);
    });
  }
}
