import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting(), { provide: Router, useValue: { navigate: () => {} } }],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('al iniciar sin token, currentUser es null', () => {
    expect(service.currentUser()).toBeNull();
    expect(service.isLoggedIn()).toBe(false);
  });

  it('login llama al endpoint y guarda el token', () => {
    const mockResponse = { token: 'abc', expiration: '', userId: 1, userName: 'test', email: 'test@test.com' };

    service.login({ email: 'test@test.com', password: '123456' }).subscribe();

    const req = httpMock.expectOne(req => req.url.includes('/Usuarios/login') && req.method === 'POST');
    req.flush(mockResponse);

    expect(service.isLoggedIn()).toBe(true);
    expect(service.getToken()).toBe('abc');
    expect(service.currentUser()?.userName).toBe('test');
  });

  it('register llama al endpoint y guarda sesión', () => {
    const mockResponse = { token: 'xyz', expiration: '', userId: 2, userName: 'newuser', email: 'new@test.com' };

    service.register({ userName: 'newuser', email: 'new@test.com', password: 'Pass123!' }).subscribe();

    const req = httpMock.expectOne(req => req.url.includes('/Usuarios/register') && req.method === 'POST');
    req.flush(mockResponse);

    expect(service.isLoggedIn()).toBe(true);
    expect(service.getToken()).toBe('xyz');
  });

  it('logout limpia sesión y redirige a login', () => {
    localStorage.setItem('mh_token', 'abc');
    localStorage.setItem('mh_user', JSON.stringify({ token: 'abc', userName: 'test', email: 'test@test.com' }));

    service.logout();

    expect(service.isLoggedIn()).toBe(false);
    expect(service.currentUser()).toBeNull();
    expect(localStorage.getItem('mh_token')).toBeNull();
  });

  it('getToken retorna null si no hay sesión', () => {
    expect(service.getToken()).toBeNull();
  });

  it('al reinstanciar con token en localStorage, recupera sesión', () => {
    localStorage.clear();
    localStorage.setItem('mh_token', 'abc');
    localStorage.setItem('mh_user', JSON.stringify({ token: 'abc', userName: 'persisted', email: 'p@test.com' }));

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting(), { provide: Router, useValue: { navigate: () => {} } }],
    });
    const freshService = TestBed.inject(AuthService);
    const freshHttpMock = TestBed.inject(HttpTestingController);

    expect(freshService.isLoggedIn()).toBe(true);
    expect(freshService.currentUser()?.userName).toBe('persisted');

    freshHttpMock.verify();
    localStorage.clear();
    TestBed.resetTestingModule();
  });
});
