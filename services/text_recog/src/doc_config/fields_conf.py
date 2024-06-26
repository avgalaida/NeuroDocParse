# src/field_transformations.py

class FieldType:
    TEXT = 'text'
    NUMBER = 'number'

class FieldTransformations:
    def __init__(self, field_type, rotate=None, binarize=False):
        self.field_type = field_type
        self.rotate = rotate
        self.binarize = binarize

DOCUMENT_FIELD_CONFIGS = {
    'passport': {
        'series': FieldTransformations(field_type=FieldType.NUMBER, rotate=-90, binarize=True),
        'number': FieldTransformations(field_type=FieldType.NUMBER, rotate=-90, binarize=True),
        'first_name': FieldTransformations(field_type=FieldType.TEXT, binarize=True),
        'last_name': FieldTransformations(field_type=FieldType.TEXT, binarize=True),
        'middle_name': FieldTransformations(field_type=FieldType.TEXT, binarize=True),
        'birth_date': FieldTransformations(field_type=FieldType.TEXT, binarize=True),
        'birth_place': FieldTransformations(field_type=FieldType.TEXT, binarize=True),
        'gender': FieldTransformations(field_type=FieldType.TEXT, binarize=True),
        # Добавьте другие поля по необходимости
    },
    # Добавьте конфигурации для других типов документов
}