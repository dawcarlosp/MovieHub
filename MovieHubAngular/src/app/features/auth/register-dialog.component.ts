import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidatorFn } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../core/services/auth.service';

interface FieldErrors {
  UserName?: string[];
  Email?: string[];
  Password?: string[];
}

function passwordStrengthValidator(): ValidatorFn {
  return (control: AbstractControl): Record<string, boolean> | null => {
    const value: string = control.value || '';
    const errors: Record<string, boolean> = {};
    if (!/[A-Z]/.test(value)) errors['uppercase'] = true;
    if (!/[a-z]/.test(value)) errors['lowercase'] = true;
    if (!/[0-9]/.test(value)) errors['digit'] = true;
    return Object.keys(errors).length ? errors : null;
  };
}

@Component({
  selector: 'app-register-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatDialogModule, MatButtonModule,
    MatFormFieldModule, MatInputModule, MatIconModule, MatTooltipModule
  ],
  templateUrl: './register-dialog.component.html',
  styleUrl: './register-dialog.component.scss'
})
export class RegisterDialogComponent {
  private readonly auth = inject(AuthService);
  private readonly dialogRef = inject(MatDialogRef<RegisterDialogComponent>);

  readonly form = new FormGroup({
    userName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(50)]
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(100)]
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(6),
        passwordStrengthValidator()
      ]
    })
  });

  fieldErrors: FieldErrors = {};
  generalError = '';
  submitting = false;
  hidePassword = true;

  get userName() { return this.form.controls.userName; }
  get email() { return this.form.controls.email; }
  get password() { return this.form.controls.password; }

  onSubmit(): void {
    this.fieldErrors = {};
    this.generalError = '';

    if (this.form.invalid) return;

    this.submitting = true;
    const { userName, email, password } = this.form.getRawValue();

    this.auth.register({ userName, email, password }).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err: HttpErrorResponse) => {
        this.submitting = false;
        if (err.error?.errors) {
          this.fieldErrors = err.error.errors;
        } else if (err.error?.message) {
          this.generalError = err.error.message;
        } else {
          this.generalError = 'Error al registrar. Inténtalo de nuevo.';
        }
      }
    });
  }

  fieldError(field: keyof FieldErrors): string | null {
    const backend = this.fieldErrors[field]?.[0];
    if (backend) return backend;

    const control = this.form.get(field);
    if (!control || !control.errors || !control.touched) return null;

    if (control.errors['required']) return 'Este campo es obligatorio.';
    if (control.errors['email']) return 'Formato de email no válido.';
    if (control.errors['minlength']) return `Mínimo ${control.errors['minlength'].requiredLength} caracteres.`;
    if (control.errors['maxlength']) return `Máximo ${control.errors['maxlength'].requiredLength} caracteres.`;
    if (control.errors['uppercase']) return 'Debe contener al menos una mayúscula.';
    if (control.errors['lowercase']) return 'Debe contener al menos una minúscula.';
    if (control.errors['digit']) return 'Debe contener al menos un dígito.';

    return null;
  }
}
