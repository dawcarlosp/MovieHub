import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-trailer-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatDialogModule, MatButtonModule,
    MatFormFieldModule, MatInputModule, MatIconModule
  ],
  templateUrl: './trailer-dialog.component.html',
  styleUrl: './trailer-dialog.component.scss'
})
export class TrailerDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<TrailerDialogComponent>);

  readonly url = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required]
  });

  openUrl(): void {
    const input = this.url.getRawValue();
    if (!input) return;
    const fullUrl = input.startsWith('http://') || input.startsWith('https://')
      ? input
      : `https://${input}`;
    window.open(fullUrl, '_blank');
    this.dialogRef.close();
  }
}
