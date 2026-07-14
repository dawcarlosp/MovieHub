import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../core/services/auth.service';
import { RegisterDialogComponent } from './register-dialog.component';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatButtonModule, MatFormFieldModule, MatInputModule, MatIconModule, MatSnackBarModule
  ],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss'
})
export class LoginPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email]
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required]
    })
  });

  submitting = false;
  hidePassword = true;

  get email() { return this.form.controls.email; }
  get password() { return this.form.controls.password; }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.submitting = true;
    const { email, password } = this.form.getRawValue();

    this.auth.login({ email, password }).subscribe({
      next: () => {
        this.snackBar.open('Sesión iniciada correctamente', '', { duration: 2000 });
        this.router.navigate(['/inicio']);
      },
      error: (err: HttpErrorResponse) => {
        this.submitting = false;
        if (err.status === 401) {
          this.snackBar.open('Email o contraseña incorrectos.', 'Cerrar', { duration: 4000 });
        } else if (err.error?.message) {
          this.snackBar.open(err.error.message, 'Cerrar', { duration: 5000 });
        } else {
          this.snackBar.open('Error al iniciar sesión. Inténtalo de nuevo.', 'Cerrar', { duration: 5000 });
        }
      }
    });
  }

  openRegister(): void {
    const ref = this.dialog.open(RegisterDialogComponent, {
      width: '420px',
      disableClose: true
    });
    ref.afterClosed().subscribe((result) => {
      if (result) this.router.navigate(['/inicio']);
    });
  }
}
