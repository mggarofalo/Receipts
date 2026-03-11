import * as React from "react"
import {
  useFormContext,
  useFormState,
  type FieldPath,
  type FieldValues,
} from "react-hook-form"

type FormFieldContextValue<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> = {
  name: TName
}

export const FormFieldContext = React.createContext<FormFieldContextValue>(
  {} as FormFieldContextValue
)

type FormItemContextValue = {
  id: string
}

export const FormItemContext = React.createContext<FormItemContextValue>(
  {} as FormItemContextValue
)

export const useFormField = () => {
  const fieldContext = React.useContext(FormFieldContext)
  const itemContext = React.useContext(FormItemContext)
  const { getFieldState } = useFormContext()
  const formState = useFormState({ name: fieldContext.name })
  const { invalid, isDirty, isTouched, isValidating, error } = getFieldState(
    fieldContext.name,
    formState,
  )

  if (!fieldContext) {
    throw new Error("useFormField should be used within <FormField>")
  }

  const { id } = itemContext

  return React.useMemo(
    () => ({
      id,
      name: fieldContext.name,
      formItemId: `${id}-form-item`,
      formDescriptionId: `${id}-form-item-description`,
      formMessageId: `${id}-form-item-message`,
      invalid,
      isDirty,
      isTouched,
      isValidating,
      error,
    }),
    [id, fieldContext.name, invalid, isDirty, isTouched, isValidating, error],
  )
}
