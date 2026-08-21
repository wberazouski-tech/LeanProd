import { Component, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
@Component({ selector: 'app-sample-form', standalone: true, imports: [ReactiveFormsModule, TranslocoPipe], templateUrl: './sample-form.component.html' })
export class SampleFormComponent { readonly saved = signal(false); readonly form = new FormGroup({ date: new FormControl('', { nonNullable: true, validators: Validators.required }), shift: new FormControl('1', { nonNullable: true, validators: Validators.required }), product: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }), quantity: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]) }); submit(): void { if (this.form.invalid) { this.form.markAllAsTouched(); return; } this.saved.set(true); } }
